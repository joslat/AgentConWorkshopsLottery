# Excel Analysis Tools

This directory contains tools to analyze the input Excel file for the Workshop Lottery System.

## Available Tools

### 1. Python Analyzer (Recommended)

**File:** `excel_analyzer.py`

**Requirements:**
- Python 3.7+
- pandas
- openpyxl

**Installation:**
```bash
pip install pandas openpyxl
```

**Usage:**
```bash
python excel_analyzer.py
```

**Features:**
- Reads Excel file structure
- Lists all column headers
- Analyzes fuzzy matching patterns (based on ADR-005)
- Shows anonymized data samples
- Checks requirements compliance

### 2. .NET Analyzer

**File:** `Program.cs` + `ExcelAnalyzer.csproj`

**Requirements:**
- .NET 8.0 SDK
- ClosedXML NuGet package

**Usage:**
```bash
cd tools
dotnet run
```

**Features:**
- Uses ClosedXML (same library as main project)
- Detailed Excel structure analysis
- Fuzzy matching pattern testing

## Output

Both tools provide:

1. **Column Headers:** Complete list with positions
2. **Row Count:** Total data rows
3. **Fuzzy Matching Analysis:** Which columns match expected patterns
4. **Data Samples:** Anonymized sample data 
5. **Requirements Compliance:** How well the file matches expected format

## Expected Output Format

Based on the requirements, the tools check for these column patterns:

- **Name:** Contains "name" (but not "email")
- **Email:** Contains "email"  
- **Laptop:** Contains "laptop"
- **Commit 10 Min:** Contains "commit", "10 min", or "early"
- **Workshop 1:** Contains "workshop 1"
- **Workshop 2:** Contains "workshop 2"  
- **Workshop 3:** Contains "workshop 3"
- **Rankings:** Contains "rank"

## Files Analyzed

- Input: `../input/AgentCon Zurich – Workshop Signup (Lottery + Standby)(1-7).xlsx`

Run either tool to get detailed analysis of the Excel file structure and compliance with the workshop lottery system requirements.