using System.Diagnostics;
using System.Security;
using CSCC;
using CSCC.CodeGen;
using CSCC.CodeGen.Emit;
using CSCC.CodeGen.Syntax;
using CSCC.Lexing;
using CSCC.Parsing;
using CSCC.Parsing.Syntax;
using CSCC.TAC;
using CSCC.TAC.Syntax;
using static ProgramFlags;
using static ProgramStatus;

var quiet = Console.IsInputRedirected && !Debugger.IsAttached;

string[] heading = [
    "CSCC - C# C Compiler (c) 2025 Josh Wyant",
    "https://github.com/joshwyant/c-compiler"
];
var version = "0.1";
var numRequiredArgs = 1;

var cts = new CancellationTokenSource();

// Argument parsing
var programFlags = None;
var nonFlags = new List<string>();
var verbose = false;
var headerShown = false;
var versionShown = false;

// Stage outputs
Token[] tokens = [];
ProgramNode? program = null;
ProgramTACNode? tacProgram = null;
ProgramAsmNode? assemblyProgram = null;

Console.CancelKeyPress += (sender, eventArgs) =>
{
    eventArgs.Cancel = true; // Don't terminate yet
    cts.Cancel(); // Use our cancellation token instead
};

try
{
    string? preprocessedFileName, assembledFileName;

    var status = ProcessArguments(out var sourceFileName, out var programName, out var outputFileName);
    if (status != Success)
    {
        Environment.Exit((int)status);
    }

    //
    // Step 1: Invoke preprocessor
    (preprocessedFileName, status) = await PreprocessAsync(sourceFileName, programName, cts.Token);
    if (status != Success || preprocessedFileName == null)
    {
        Environment.Exit((int)status);
    }

    //
    // Step 2: Compile the program
    (assembledFileName, status) = await CompileAsync(preprocessedFileName, programName, cts.Token);
    if (status != Success || assembledFileName == null)
    {
        Environment.Exit((int)status);
    }

    // Skip step 3?
    if (programFlags.HasFlag(OutputAssembly))
    {
        // -S flag
        // Output assembly (.s file) only, and don't assemble or link;
        return;
    }

    //
    // Step 3: Assemble and link
    status = await AssembleAsync(assembledFileName, programName, outputFileName, cts.Token);
    if (status != Success)
    {
        Environment.Exit((int)status);
    }
}
catch (Exception e)
{
    Error(UnknownError, $"Unhandled exception: {e}", exit: true);
}

async Task<(string? preprocessedFileName, ProgramStatus)> PreprocessAsync(string sourceFileName, string programName, CancellationToken cancellationToken = default)
{

    if (verbose) Console.WriteLine("Starting preprocessor...");

    var preprocessedFileName = $"{programName}.i";
    var program = "gcc";
    // -E: Invoke the preprocessor
    // -P: Don't emit linemarkers
    var args = $"-E -P {sourceFileName} -o {preprocessedFileName}";
    if (verbose) args += " --verbose";

    if (verbose)
    {
        Console.WriteLine($"{program} {args}");
    }

    var preprocessor = Process.Start(
        new ProcessStartInfo
        {
            FileName = program,
            Arguments = args,
        });

    if (preprocessor == null)
    {
        return (null, Error(ProcessFail, "Failed to invoke the preprocessor."));
    }

    await preprocessor.WaitForExitAsync(cancellationToken);

    if (preprocessor.ExitCode != 0)
    {
        return (null, Error(PreprocessorFailed, $"Preprocessor failed with exit code {preprocessor.ExitCode}."));
    }

    if (verbose) Console.WriteLine("Preprocessor complete.");

    return (preprocessedFileName, Success);
}

async Task<(string? assembledFileName, ProgramStatus)> CompileAsync(string preprocessedFileName, string programName, CancellationToken cancellationToken = default)
{
    var assembledFileName = string.Empty;

    // Perform lexical analysis
    var status = await LexAsync(preprocessedFileName, cancellationToken);
    // --lex flag means stop after lexing.
    if (status != Success || programFlags.HasFlag(StopAfterLex))
    {
        return (null, status);
    }

    // Now parse
    status = await ParseAsync(cancellationToken);
    // --parse flag means stop after parsing.
    if (status != Success || programFlags.HasFlag(StopAfterParse))
    {
        return (null, status);
    }

    // Now generate three-address code
    status = await TACGenAsync(cancellationToken);
    // --tacky flag means stop after TAG generation.
    if (status != Success || programFlags.HasFlag(StopAfterTacky))
    {
        return (null, status);
    }

    // Now perform code generation
    status = await CodeGenAsync(cancellationToken);
    // --codegen flag means stop after code generation.
    if (status != Success || programFlags.HasFlag(StopAfterCodeGen))
    {
        return (null, status);
    }

    // Now generate assembly file
    return await EmitAsync(programName, cancellationToken);
}

async Task<ProgramStatus> LexAsync(string preprocessedFileName, CancellationToken cancellationToken = default)
{
    if (verbose) Console.WriteLine("\nPerforming lexical analysis...");

    try
    {
        var lexer = new Lexer(preprocessedFileName);

        // No streaming in v1 to simplify debugging.
        tokens = await lexer.ToArrayAsync(cancellationToken);

        if (verbose)
        {
            Console.WriteLine($"Tokens found: [{string.Join(", ", tokens.Select(t => $"\"{t}\""))}]");
        }

        if (lexer.Errors.Count != 0)
        {
            foreach (var error in lexer.Errors)
            {
                Console.Error.WriteLine($"Error: {error}");
            }
            return Error(CompilerError, "Lexical errors found");
        }
    }
    catch (IOException e)
    {
        return Error(IOError, $"IO Error occurred while lexing: {e.Message}");
    }
    finally
    {
        // Finished with the preprocessed file
        if (verbose) Console.WriteLine($"Deleting {preprocessedFileName}...");
        File.Delete(preprocessedFileName);
    }

    if (verbose) Console.WriteLine("Lexical analysis complete.");

    return Success;
}

async Task<ProgramStatus> ParseAsync(CancellationToken cancellationToken = default)
{
    if (verbose) Console.WriteLine("\nParsing...");

    // TODO: Actually parse.
    var parser = new Parser(tokens);
    program = await parser.ParseAsync(cancellationToken);

    if (verbose)
    {
        Console.WriteLine("Abstract syntax tree:");
        Console.WriteLine(program);
    }

    if (parser.Errors.Count != 0)
    {
        foreach (var error in parser.Errors)
        {
            Console.Error.WriteLine($"Error: {error}");
        }
        return Error(CompilerError, "Syntax errors found");
    }

    if (verbose) Console.WriteLine("Parsing complete.");

    return Success;
}

Task<ProgramStatus> TACGenAsync(CancellationToken cancellationToken = default)
{
    if (verbose) Console.WriteLine("\nGenerating three-address code (TAC)...");

    if (program == null)
    {
        return Task.FromResult(Error(CompilerError, "No valid program!"));
    }

    var generator = new TACGenerator(program);
    tacProgram = generator.Generate(program);

    if (verbose)
    {
        Console.WriteLine(tacProgram);
    }

    if (verbose) Console.WriteLine("TAC generation complete.");

    return Task.FromResult(Success);
}

Task<ProgramStatus> CodeGenAsync(CancellationToken cancellationToken = default)
{
    if (verbose) Console.WriteLine("\nGenerating code...");

    if (tacProgram == null)
    {
        return Task.FromResult(Error(CompilerError, "No valid three-address code!"));
    }

    var generator = new CodeGenerator(tacProgram);
    assemblyProgram = generator.Generate();

    if (verbose)
    {
        Console.WriteLine(assemblyProgram);
    }

    if (verbose) Console.WriteLine("Code generation complete.");

    return Task.FromResult(Success);
}

async Task<(string? assembledFileName, ProgramStatus)> EmitAsync(string programName, CancellationToken cancellationToken = default)
{
    if (verbose) Console.WriteLine("\nGenerating assembly...");

    var assembledFileName = $"{programName}.s";

    // For now, just output a fake one
    var status = Success;
    try
    {
        if (verbose) Console.WriteLine($"Creating {assembledFileName}...");

        if (assemblyProgram == null)
        {
            return (null, Error(CompilerError, "No code was generated!"));
        }

        var writer = new AssemblyWriter(assemblyProgram);
        await writer.WriteAsync(assembledFileName, cancellationToken);

        if (verbose)
        {
            Console.WriteLine(File.ReadAllText(assembledFileName));
        }

        if (verbose) Console.WriteLine("Emit assembly complete.");
    }
    catch
    {
        status = AssemblyFailed;
    }
    finally
    {
        if (status != Success)
        {
            try
            {
                if (verbose) Console.WriteLine($"Deleting {assembledFileName}...");
                if (File.Exists(assembledFileName)) File.Delete(assembledFileName);
            }
            catch (Exception e)
            {
                if (verbose) Console.WriteLine($"Failed to delete {assembledFileName}: {e}");
            }
        }
    }

    return (status == Success ? assembledFileName : null, status);
}

async Task<ProgramStatus> AssembleAsync(string assembledFileName, string programName, string outputFileName, CancellationToken cancellationToken = default)
{
    if (verbose) Console.WriteLine("\nAssembling and linking...");

    var status = Success;
    var program = "gcc";
    var args = $"{assembledFileName} -o {outputFileName}";
    if (OperatingSystem.IsMacOS()) args += " -arch x86_64";
    if (verbose) args += " --verbose";

    if (verbose) Console.WriteLine($"{program} {args}");

    async Task<ProgramStatus> RunGCCAsync()
    {
        var gcc = Process.Start(
            new ProcessStartInfo
            {
                FileName = program,
                Arguments = args,
            });

        if (gcc == null)
        {
            return Error(ProcessFail, "Failed to invoke gcc to run assembler.");
        }

        await gcc.WaitForExitAsync(cancellationToken);

        if (gcc.ExitCode != 0)
        {
            return Error(AssemblyFailed, $"Assembler failed with exit code {gcc.ExitCode}.");
        }

        return Success;
    }

    try
    {
        status = await RunGCCAsync();
    }
    catch
    {
        status = AssemblyFailed;
    }
    finally
    {
        if (File.Exists(assembledFileName))
        {
            if (verbose) Console.WriteLine($"Deleting {assembledFileName}...");
            File.Delete(assembledFileName);
        }
    }

    if (status != Success)
    {
        return Error(status, "Assembly failed");
    }

    if (verbose)
    {
        Console.WriteLine($"Successfully created \"{programName}\".");
    }

    if (verbose) Console.WriteLine("Assembly and linking complete.");

    return Success;
}

void ValidateArgs()
{
    // Validate arguments (conflicting flags, etc.)
    ProgramFlags[] modes = [
        StopAfterLex,
        StopAfterParse,
        StopAfterCodeGen,
        StopAfterTacky,
        OutputAssembly
    ];
    ProgramFlags mode = None;
    bool SetMode(ProgramFlags f)
    {
        ProgramFlags prev;
        (mode, prev) = (f, mode);
        return prev == None;
    }
    foreach (var m in modes)
    {
        if (programFlags.HasFlag(m) && !SetMode(m))
        {
            Usage(ConflictingFlags, "Conflicting flags used.", exit: true);
            return;
        }
    }
}

void ShowVersion()
{
    if (versionShown == false)
    {
        ShowHeader();
        Console.WriteLine($"Version {version}");
    }
    versionShown = true;
}

void ProcessMiscFlags()
{
    if (programFlags.HasFlag(GetVersion))
    {
        ShowVersion();
        Environment.Exit(0);
    }
    else if (programFlags.HasFlag(Help))
    {
        Usage(exit: true);
    }

    if (programFlags.HasFlag(Verbose))
    {
        verbose = true;
        ShowHeader();
        ShowVersion(); // be verbose about it
    }
}

ProgramStatus Error(ProgramStatus status, string message, bool exit = false)
{
    Console.Error.WriteLine($"Error: {message}");

    if (exit)
    {
        Environment.Exit((int)status);
    }

    return status;
}

void ShowHeader()
{
    if (headerShown) return;

    // Copyright, etc.
    foreach (var headingLine in heading)
    {
        Console.WriteLine(headingLine);
    }

    headerShown = true;
}

ProgramStatus Usage(ProgramStatus status = Success, string? message = null, bool exit = false)
{
    if (!quiet && !programFlags.HasFlag(Quiet) && status == Success)
    {
        ShowHeader();
    }

    var writer = status == Success ? Console.Out : Console.Error;
    if (message != null)
    {
        writer.WriteLine($"{(status != Success ? "Error: " : "")}{message}");
    }

    Console.WriteLine("Usage: CSCC [options] <source-file>");
    Console.WriteLine("Options:");
    Console.WriteLine("  --lex           Stop after lexical analysis");
    Console.WriteLine("  --parse         Stop after parsing");
    Console.WriteLine("  --tacky         Stop after TAC generation");
    Console.WriteLine("  --codegen       Stop after code generation");
    Console.WriteLine("  -S              Output assembly only (no assembling/linking)");
    Console.WriteLine("  --help          Show this help message");
    Console.WriteLine("  --version       Display compiler version");
    Console.WriteLine("  --verbose       Verbose mode");
    Console.WriteLine("  --quiet         Suppress output");

    if (exit)
    {
        Environment.Exit((int)status);
    }

    return status;
}

ProgramStatus ProcessArguments(out string sourceFileName, out string programName, out string outputFileName)
{
    // We'll figure out the source file name and set the program
    // name to the same filename but without the extension.
    sourceFileName = string.Empty;
    programName = string.Empty;
    outputFileName = string.Empty;

    // Get the flags
    foreach (var arg in args)
    {
        var nextFlag = arg switch
        {
            "--lex" => StopAfterLex,
            "--parse" => StopAfterParse,
            "--tacky" => StopAfterTacky,
            "--codegen" => StopAfterCodeGen,
            "-S" => OutputAssembly,
            "--quiet" => Quiet,
            "--help" => Help,
            "--verbose" => Verbose,
            "--version" => GetVersion,
            _ => None
        };
        programFlags |= nextFlag;

        if (nextFlag != None) continue;

        nonFlags.Add(arg);
    }

    foreach (var arg in nonFlags)
    {
        if (arg.StartsWith('-'))
        {
            return Usage(InvalidFlag, $"The specified flag '{arg}' is invalid.");
        }
    }

    ProcessMiscFlags();

    // Need at least the number of required args
    if (nonFlags.Count != numRequiredArgs)
    {
        // TODO: This should be an error if other flags exist.
        return Usage(exit: true);
    }

    // The main non-flag argument is the name of the source file.
    sourceFileName = nonFlags[0];

    FileInfo? fi = null;
    try
    {
        fi = new FileInfo(sourceFileName);
    }
    catch (SecurityException e)
    {
        return Error(SecurityIssue, $"Security issue: {e}");
    }
    catch (UnauthorizedAccessException)
    {
        return Error(UnauthorizedAccess, $"Access to '{sourceFileName}' denied. Ensure you have the proper privileges.");
    }
    catch (NotSupportedException)
    {
        return Usage(InvalidFilename, $"File name '{sourceFileName}' is invalid.");
    }

    if (!fi.Exists || fi.DirectoryName == null)
    {
        return Usage(FileNotFound, $"Source file '{sourceFileName}' does not exist.");
    }

    programName = Path.GetFileNameWithoutExtension(sourceFileName);
    outputFileName = Path.Combine(fi.DirectoryName, programName);

    // Check for conflicting flags, etc.
    ValidateArgs();

    return Success;
}

[Flags]
enum ProgramFlags
{
    None = 0,
    StopAfterLex = 1,
    StopAfterParse = 1 << 1,
    StopAfterCodeGen = 1 << 2,
    OutputAssembly = 1 << 3,
    Quiet = 1 << 4,
    Help = 1 << 5,
    Verbose = 1 << 6,
    GetVersion = 1 << 7,
    StopAfterTacky = 1 << 8,
}

enum ProgramStatus
{
    Success = 0,
    GeneralError = 1,
    UnknownError,
    InvalidFlag,
    FileNotFound,
    SecurityIssue,
    UnauthorizedAccess,
    InvalidFilename,
    ProcessFail,
    PreprocessorFailed,
    AssemblyFailed,
    ConflictingFlags,
    CompilerError,
    IOError,
}