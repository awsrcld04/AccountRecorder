
# AccountRecorder

DESCRIPTION: 
- Generate a list of user accounts in the domain and account group membership

> NOTES: "v1.0" was completed in 2011. AccountRecorder was written to work in on-premises Active Directory environments. The purpose of AccountRecorder was/is to create a list of user accounts and the associated group membership mainly for data protection (backup/recovery). The list can also be used for auditing.

## Requirements:

Operating System Requirements:
- Windows Server 2003 or higher (32-bit)
- Windows Server 2008 or higher (32-bit)

Additional software requirements:
Microsoft .NET Framework v3.5

Active Directory requirements:
One of following domain functional levels
- Windows Server 2003 domain functional level
- Windows Server 2008 domain functional level

Additional requirements:
Domain administrative access is required to perform operations by AccountRecorder


## Operation and Configuration:

Command-line parameters:
- run (Required parameter)
- domain (Reference the domain the computer is a member of)

Configuration file: none

Output:
- Located in the Log directory inside the installation directory; log files are in tab-delimited format
- Path example: (InstallationDirectory)\Log\AR[Date]\

Additional detail:
AccountRecorder is “read-only” and reads information from Active Directory.
