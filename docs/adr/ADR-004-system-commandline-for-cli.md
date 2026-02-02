# ADR-004: System.CommandLine for CLI Parsing

**Status:** Accepted  
**Date:** 2026-02-02  
**Context:** Command-line argument parsing library

---

## Context

The application is a CLI tool requiring parsing of:
- Required arguments (input path)
- Optional arguments with defaults (output path, capacity, seed, order)
- Help generation
- Validation

## Decision Drivers

- Modern .NET ecosystem compatibility
- Automatic help generation
- Type-safe argument binding
- Validation support
- Minimal boilerplate
- Microsoft ecosystem alignment

## Options Considered

### Option 1: System.CommandLine
- Microsoft's modern CLI library
- Released stable in 2023
- Rich feature set
- Convention-based binding

### Option 2: CommandLineParser
- Popular community library
- Attribute-based
- Mature but older patterns

### Option 3: Spectre.Console.Cli
- Part of Spectre.Console
- Beautiful output
- More ceremony for simple CLIs

### Option 4: Manual args[] Parsing
- No dependencies
- Full control
- Significant boilerplate

## Decision

**Option 1: System.CommandLine**

## Rationale

1. **Microsoft Supported:** Part of .NET ecosystem, long-term viability
2. **Modern Patterns:** Async support, DI-friendly
3. **Automatic Features:**
   - Help text from descriptions
   - Tab completion
   - Typo suggestions
4. **Clean Definition:**
   ```csharp
   var inputOption = new Option<FileInfo>(
       name: "--input",
       description: "Input Excel file from MS Forms") 
       { IsRequired = true };
   
   var capacityOption = new Option<int>(
       name: "--capacity",
       getDefaultValue: () => 34,
       description: "Seats per workshop");
   ```

5. **Validation Built-in:**
   ```csharp
   inputOption.AddValidator(result => {
       var file = result.GetValueForOption(inputOption);
       if (!file.Exists)
           result.ErrorMessage = "Input file not found";
   });
   ```

## Consequences

### Positive
- Professional CLI experience
- Minimal code for argument handling
- Consistent with .NET ecosystem
- Built-in validation pipeline

### Negative
- Dependency added (acceptable for CLI tools)
- Learning curve for advanced scenarios (not needed here)

### Usage Example
```bash
workshop-lottery --input registrations.xlsx --capacity 30 --seed 42

workshop-lottery --help
# Outputs formatted help with all options
```

## Related Documents
- [ARCHITECTURE.md](../ARCHITECTURE.md) - CLI specification in Section 7
