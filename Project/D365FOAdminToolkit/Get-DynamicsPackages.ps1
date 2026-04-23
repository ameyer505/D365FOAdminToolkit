# Copies the following nessesary files to the folder Packages\DynamicsTemp
# - Microsoft.Dynamics.AX.Security.Management
# - Microsoft.Dynamics.AX.Metadata.Core
# In order to overcome differences between Local VHD VMs, Cloud Hosted Environments and Universal Development Environments


param (
    [Parameter(Mandatory=$true)]
    [string]$PackagesLocalDirectoryPath,

    [Parameter(Mandatory=$true)]
    [string]$RepoBasePath
)


Copy-Item -Path "$PackagesLocalDirectoryPath\bin\Microsoft.Dynamics.AX.Metadata.dll" -Destination "$RepoBasePath\Project\D365FOAdminToolkit\packages\DynamicsTemp"
Copy-Item -Path "$PackagesLocalDirectoryPath\bin\Microsoft.Dynamics.AX.Metadata.Core.dll" -Destination "$RepoBasePath\Project\D365FOAdminToolkit\packages\DynamicsTemp"
Copy-Item -Path "$PackagesLocalDirectoryPath\bin\Microsoft.Dynamics.AX.Metadata.Modeling.dll" -Destination "$RepoBasePath\Project\D365FOAdminToolkit\packages\DynamicsTemp"
Copy-Item -Path "$PackagesLocalDirectoryPath\bin\Microsoft.Dynamics.AX.Xpp.AxShared.dll" -Destination "$RepoBasePath\Project\D365FOAdminToolkit\packages\DynamicsTemp"
Copy-Item -Path "$PackagesLocalDirectoryPath\bin\Microsoft.Dynamics.AX.Xpp.Support.dll" -Destination "$RepoBasePath\Project\D365FOAdminToolkit\packages\DynamicsTemp"
Copy-Item -Path "$PackagesLocalDirectoryPath\bin\Microsoft.Dynamics.AX.Security.Management.dll" -Destination "$RepoBasePath\Project\D365FOAdminToolkit\packages\DynamicsTemp"

###########################################
# Linking Metadata folders for UDE 


new-item -itemtype symboliclink -path "C:\CustomXppMetadatawdk4qc01.rsj" -name D365FOAdminToolkit -value "$RepoBasePath\Metadata"
new-item -itemtype symboliclink -path "C:\CustomXppMetadatawdk4qc01.rsj" -name D365FOAdminToolkitTests -value "$RepoBasePath\Metadata"