# What each supported build targets. Dot-sourced by package.ps1.
#
# The package VERSION is not defined here -- it is derived by the build, in
# Directory.Build.targets, and read back off the compiled assembly by
# package.ps1. Keeping one source for it means the stamped assemblies,
# pkg.json and the zip names cannot drift apart.
#
# Dynamo - the DynamoRevit version. Names the packages folder under
#          %AppData%\Dynamo\Dynamo Revit\<version>\packages.
# Engine - the DynamoCore version. What pkg.json's engine_version means, and
#          what the DynamoVisualProgramming.* packages track. This is what
#          routes each user to the right build, so it must match the Dynamo
#          the project is actually compiled against.
# Tfm    - target framework, which follows the .NET runtime the Revit release
#          hosts. A mid-cycle Revit update that changes runtime needs its own
#          entry and its own project pair, not an edit to an existing one.
#
# DynamoRevit and DynamoCore were the same number until Revit 2027, where
# DynamoRevit switched to year-based versioning (27.0) while DynamoCore
# continued as 4.0.
$ArchilabTargets = @{
    2025 = @{ Dynamo = '3.2';  Engine = '3.2.1.5366'; Tfm = 'net8.0-windows'  }
    2026 = @{ Dynamo = '3.6';  Engine = '3.6.1.9895'; Tfm = 'net8.0-windows'  }
    2027 = @{ Dynamo = '27.0'; Engine = '4.0.2.3852'; Tfm = 'net10.0-windows' }
}
