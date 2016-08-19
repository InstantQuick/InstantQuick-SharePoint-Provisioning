$scriptdir = $PSScriptRoot
$clientContext
$rootSiteUrl
[Reflection.Assembly]::LoadFrom("$scriptdir\assemblies\IQAppManifestProvisioner.dll")
[Reflection.Assembly]::LoadFrom("$scriptdir\assemblies\IQAppSiteProvisioner.dll")
[Reflection.Assembly]::LoadFrom("$scriptdir\assemblies\IQAppManifestBuilders.dll")
[Reflection.Assembly]::LoadFrom("$scriptdir\assemblies\IQAppProvisioningBaseClasses.dll")
[Reflection.Assembly]::LoadFrom("$scriptdir\assemblies\Microsoft.SharePoint.Client.dll")
[Reflection.Assembly]::LoadFrom("$scriptdir\assemblies\IQAppStorageMigrator.dll")

Add-Type -AssemblyName System.Xml.Linq

<#
    .SYNOPSIS
    Creates an instance of an AppManifestBase object from scratch or from JSON

	.DESCRIPTION
	New-IQAppManifest returns an object of type IQAppProvisioningBaseClasses.Provisioning.AppManifestBase as a new object or from JSON.

	The JSON can be in the form of a string or a file located in a file system volume or Azure Storage.
	
	If the file is located in Azure storage, it must be named manifest.json. 
#>
function New-IQAppManifest
{
	[CmdletBinding()]
	param
	(
	    [Parameter()]
	    [string]$JSON,
		
		[Parameter()]
		[string]$JSONFilePath,
				
		[Parameter()]
	    [bool]$PreserveBaseFilePath,

		[Parameter()]
	    [string]$StorageAccount,

		[Parameter()]
	    [string]$AccountKey,

		[Parameter()]
	    [string]$Container
	)
	$Error.Clear()

	if(($JSON -eq "") -and ($JSONFilePath -eq "") -and ($StorageAccount -eq "")) 
	{
		$manifest = New-Object IQAppProvisioningBaseClasses.Provisioning.AppManifestBase
		$manifest.StorageType = [IQAppProvisioningBaseClasses.Provisioning.StorageTypes]::FileSystem
		return $manifest
	}
	elseif ($JSON -ne "")
	{
		return [IQAppProvisioningBaseClasses.Provisioning.AppManifestBase]::GetManifestFromJSON($JSON)
	}
	elseif ($JSONFilePath -ne "")
	{	
		Write-Host $JSONFilePath
		$j = Get-Content -Path "$JSONFilePath" -Encoding UTF8
		$manifest = [IQAppProvisioningBaseClasses.Provisioning.AppManifestBase]::GetManifestFromJSON($j)
		if($PreserveBaseFilePath -ne $true)
		{
			$manifest.BaseFilePath = Split-Path -Path $JSONFilePath -Parent
		}
		return $manifest
	}
	elseif (($StorageAccount -ne "") -and ($AccountKey -ne "") -and ($Container -eq ""))
	{	
		$manifest = New-Object IQAppProvisioningBaseClasses.Provisioning.AppManifestBase
		$manifest.StorageType = [IQAppProvisioningBaseClasses.Provisioning.StorageTypes]::AzureStorage
		$newContainer = ($manifest.ManifestId + "-" + $manifest.Version.ToString()).Replace(".", "-");
		$manifest.SetAzureStorageInfo($StorageAccount, $AccountKey, $newContainer)
		Save-IQAppManifest $manifest
		return $manifest
	}
	else
	{
		return [IQAppProvisioningBaseClasses.Provisioning.AppManifestBase]::GetManifestFromAzureStorage($StorageAccount, $AccountKey, $Container)
	}
}

<#
    .SYNOPSIS
    Saves an [IQAppProvisioningBaseClasses.Provisioning.AppManifestBase] instance as serialized JSON to disk or to Azure storage.

	.DESCRIPTION
	Save-IQAppManifest uses the StorageType property to determine how to save the manifest.
	
	Valid values are defined by the [IQAppProvisioningBaseClasses.Provisioning.StorageTypes] enum and are [IQAppProvisioningBaseClasses.Provisioning.StorageTypes]::FileSystem (1) and [IQAppProvisioningBaseClasses.Provisioning.StorageTypes]::AzureStorage (2).

	If the value is FileSystem (1) you must set the AppManifestBase.BaseFilePath property to a valid path prior to using this command.

	If the value is AzureStorage (2) you must call the AppManifestBase.SetAzureStorageInfo method prior to using this command. 
	
	The generated file will be named manifest.json. If a manifest.json already exists at the specified location it will be overwritten.
#>
function Save-IQAppManifest
{
	[CmdletBinding()]
	param
	(
	    [Parameter()]
	    [IQAppProvisioningBaseClasses.Provisioning.AppManifestBase]$AppManifest
	)

	$Error.Clear()
	if($AppManifest.GetAzureStorageInfo() -ne $null){
		return [IQAppProvisioningBaseClasses.Provisioning.AppManifestBase]::SaveManifestToAzureStorage($AppManifest)
	}
	else {
		return [IQAppProvisioningBaseClasses.Provisioning.AppManifestBase]::SaveManifestToFileSystem($AppManifest)
	}

}

<#
    .SYNOPSIS
    Connects to SharePoint 2013, SharePoint 2016, or SharePoint Online and returns a new Microsoft.SharePoint.Client.ClientContext instance.

	.DESCRIPTION
	Connects to SharePoint 2013, SharePoint 2016, or SharePoint Online and returns a new Microsoft.SharePoint.Client.ClientContext instance. The new ClientContext will contain fully loaded Site and Web properties.
	
	Most operations in this provisioning module require an Microsoft.SharePoint.Client.ClientContext instance and those that do assume the Site and Web properties are loaded and available. 
#>
function New-SPClientContext
{
	[CmdletBinding()]
	param
	(
	    [Parameter(Mandatory=$true, Position=1)]
	    [string]$SiteURL,
		
		[Parameter(Mandatory=$false, Position=2)]
		[switch]$Online,

	    [Parameter(Mandatory=$true, Position=3)]
	    [string]$UserName,

	    [Parameter(Mandatory=$true, Position=4)]
	    [string]$Password
	)

    $Error.Clear()

	$context = New-Object Microsoft.SharePoint.Client.ClientContext($siteURL)
	$context.RequestTimeOut = 1000 * 60 * 10;

	if ($online)
	{
		$context.AuthenticationMode = [Microsoft.SharePoint.Client.ClientAuthenticationMode]::Default
		$securePassword = ConvertTo-SecureString $password -AsPlainText -Force

		$credentials = New-Object Microsoft.SharePoint.Client.SharePointOnlineCredentials($UserName, $securePassword)
		$context.Credentials = $credentials
	}
	else
	{
		$credentials = New-Object System.Net.NetworkCredential($UserName, $Password)
		$context.Credentials = $credentials
	}
	$web = $context.Web
	$site = $context.Site
	$context.Load($web)
	$context.Load($site)
	$context.ExecuteQuery()

    return $context
}

<#
    .SYNOPSIS
    Provisions items in a SharePoint site based on the contents of the manifest and any associated files.

	.DESCRIPTION
	Provisions items in a SharePoint site based on the contents of the manifest and any associated files. 

	The manifest can either be provided using an existing instance of IQAppProvisioningBaseClasses.Provisioning.AppManifestBase or be read from a file. 

	If provisioning from Azure Storage, you must first load the manifest using the New-IQAppManifest command.
#>
function Install-IQAppManifest
{
	[CmdletBinding()]
	param
	(
	    [Parameter(Mandatory=$true, Position=1)]
	    [Microsoft.SharePoint.Client.ClientContext]$ClientContext,
		
		[Parameter(Mandatory=$true, Position=2)]
		[Microsoft.SharePoint.Client.Web]$Web,

	    [Parameter()]
	    [string]$AbsoluteJSONPath,

		[Parameter()]
	    [IQAppProvisioningBaseClasses.Provisioning.AppManifestBase]$AppManifest,

		[Parameter()]
		[switch]$VerboseNotify
	)
	$Error.Clear()
    
	$provisioner = New-Object IQAppManifestProvisioner.Provisioner
    
	if($VerboseNotify)
	{
		$provisioner.WriteNotificationsToStdOut = $true
	}

	if($AppManifest -eq $null)
	{
		Write-Host $provisioner.Provision($ClientContext, $Web, $AbsoluteJSONPath)
	}
	else
	{
		Write-Host $provisioner.Provision($ClientContext, $Web, $AppManifest)
	}
}

<#
    .SYNOPSIS
    Removes items from a SharePoint site based on the contents of the manifest.

	.DESCRIPTION
	Removes items from a SharePoint site based on the contents of the manifest.

	Note that this operation is destructive and should be used with extreme caution.

	Files that are not part of a document library are only removed if the FileCreator.DeleteOnCleanup property is set to true.

	All content in lists or libraries included in the manifest is deleted.
#>
function Uninstall-IQAppManifest
{
	[CmdletBinding()]
	param
	(
	    [Parameter(Mandatory=$true, Position=1)]
	    [Microsoft.SharePoint.Client.ClientContext]$ClientContext,
		
		[Parameter(Mandatory=$true, Position=2)]
		[Microsoft.SharePoint.Client.Web]$Web,

	    [Parameter(Position=3)]
	    [string]$AbsoluteJSONPath,

		[Parameter()]
	    [IQAppProvisioningBaseClasses.Provisioning.AppManifestBase]$AppManifest,

		[Parameter()]
		[switch]$VerboseNotify,

		[Parameter()]
	    [switch]$Force
	)
	$Error.Clear()
    
	if($Force -ne $true)
	{
		$message  = 'DANGER!!!'
		$question = 'This operation can not be undone and may remove lists and libraries from the site and all associated content depending on the content of the manifest. Are you sure you want to proceed?'

		$choices = New-Object Collections.ObjectModel.Collection[Management.Automation.Host.ChoiceDescription]
		$choices.Add((New-Object Management.Automation.Host.ChoiceDescription -ArgumentList '&Yes'))
		$choices.Add((New-Object Management.Automation.Host.ChoiceDescription -ArgumentList '&No'))

		$decision = $Host.UI.PromptForChoice($message, $question, $choices, 1)
	}
	if ($Force -eq $true -or $decision -eq 0) {
		$provisioner = New-Object IQAppManifestProvisioner.Provisioner
		if($VerboseNotify)
		{
			$provisioner.WriteNotificationsToStdOut = $true
		}
		if($AppManifest -eq $null)
		{
			Write-Host $provisioner.Deprovision($ClientContext, $Web, $AbsoluteJSONPath)
		}
		else
		{
			Write-Host $provisioner.Deprovision($ClientContext, $web, $AppManifest)
		}
	}
}

<#
    .SYNOPSIS
    Reads a field from the root web of a SharePoint site collection and optionally adds it to a manifest.

	.DESCRIPTION
	Reads a fieldfrom the root web of a SharePoint site collection and optionally adds it to a manifest. For more detail about the operation as it executes, include the -VerboseNotify switch.
#>
function Get-FieldCreator
{
	[CmdletBinding()]
	param
	(
	    [Parameter(Mandatory=$true, Position=1)]
	    [Microsoft.SharePoint.Client.ClientContext]$ClientContext,
		
		[Parameter(Mandatory=$true, Position=2)]
		[string]$FieldName,

	    [Parameter()]
		[IQAppProvisioningBaseClasses.Provisioning.AppManifestBase]$AppManifest,

		[Parameter()]
		[switch]$VerboseNotify
	)
	$Error.Clear()
    $builder = New-Object IQAppManifestBuilders.CreatorBuilder
	
    if($VerboseNotify)
	{
		$builder.WriteNotificationsToStdOut = $true
	}
	$builder.GetCreator($ClientContext, $null, $FieldName, $AppManifest, [IQAppManifestBuilders.CreatorTypes]::Field)
}

<#
    .SYNOPSIS
    Outputs Xml as formatted text 

	.DESCRIPTION
	Outputs Xml as formatted text. Use to make field, web part, views and other's readable.
#>
function Format-Xml 
{
    [CmdletBinding()]
    Param 
    (
        [parameter(ValueFromPipeline)]
        [string]$Xml
    ) 

    if($Xml -ne $null)
    {
        $XDoc = New-Object System.Xml.Linq.XDocument 
        $Reader = New-Object System.IO.StringReader($Xml)
        $XDoc = [System.Xml.Linq.XDocument]::Load($Reader) 

        $XDoc.ToString()
		$Reader.Dispose()    
    }
    else
    {
        $null
    }
}

<#
    .SYNOPSIS
    Reads a content type from the root web of a SharePoint site collection and optionally adds it to a manifest.

	.DESCRIPTION
	Reads a content type from the root web of a SharePoint site collection and optionally adds it to a manifest. For more detail about the operation as it executes, include the -VerboseNotify switch.
#>
function Get-ContentTypeCreator
{
	[CmdletBinding()]
	param
	(
	    [Parameter(Mandatory=$true, Position=1)]
	    [Microsoft.SharePoint.Client.ClientContext]$ClientContext,
		
		[Parameter(Mandatory=$true, Position=2)]
		[string]$ContentTypeName,

	    [Parameter()]
	    [IQAppProvisioningBaseClasses.Provisioning.AppManifestBase]$AppManifest,

		[Parameter()]
		[switch]$VerboseNotify
	)
	$Error.Clear()
    $builder = New-Object IQAppManifestBuilders.CreatorBuilder
	
    if($VerboseNotify)
	{
		$builder.WriteNotificationsToStdOut = $true
	}
	$builder.GetCreator($ClientContext, $null, $ContentTypeName, $AppManifest, [IQAppManifestBuilders.CreatorTypes]::ContentType)
}

<#
    .SYNOPSIS
    Reads a list definition from a SharePoint site and optionally adds it to a manifest.

	.DESCRIPTION
	Reads a list definition from a SharePoint site and optionally adds it to a manifest. For more detail about the operation as it executes, include the -VerboseNotify switch.
#>
function Get-ListCreator
{
	[CmdletBinding()]
	param
	(
	    [Parameter(Mandatory=$true, Position=1)]
	    [Microsoft.SharePoint.Client.ClientContext]$ClientContext,
		
		[Parameter(Mandatory=$true, Position=2)]
		[Microsoft.SharePoint.Client.Web]$Web,

		[Parameter(Mandatory=$true, Position=3)]
		[string]$ListName,

	    [Parameter()]
	    [IQAppProvisioningBaseClasses.Provisioning.AppManifestBase]$AppManifest,

		[Parameter()]
		[switch]$VerboseNotify
	)
	$Error.Clear()
    $builder = New-Object IQAppManifestBuilders.CreatorBuilder
	
	if($VerboseNotify)
	{
		$builder.WriteNotificationsToStdOut = $true
	}
    $builder.GetCreator($ClientContext, $web, $ListName, $AppManifest, [IQAppManifestBuilders.CreatorTypes]::List)
}

<#
    .SYNOPSIS
    Fetches up to 2000 items from an existing list and adds the metadata to an existing list creator in the manifest.

	.DESCRIPTION
	Fetches up to 2000 items from an existing list and adds the metadata to an existing list creator in the manifest. For more detail about the operation as it executes, include the -VerboseNotify switch.
#>
function Get-ListCreatorListItems
{
	[CmdletBinding()]
	param
	(
	    [Parameter(Mandatory=$true, Position=1)]
	    [Microsoft.SharePoint.Client.ClientContext]$ClientContext,
		
		[Parameter(Mandatory=$true, Position=2)]
		[Microsoft.SharePoint.Client.Web]$Web,

		[Parameter(Mandatory=$true, Position=3)]
		[string]$ListName,

		[Parameter(Mandatory=$true, Position=4)]
	    [IQAppProvisioningBaseClasses.Provisioning.AppManifestBase]$AppManifest,

		[Parameter()]
		[switch]$VerboseNotify
	)
	$Error.Clear()
	$builder = New-Object IQAppManifestBuilders.ListCreatorBuilder

	if($VerboseNotify)
	{
		$builder.WriteNotificationsToStdOut = $true
	}
	$builder.GetListCreatorListItems($ClientContext, $web, $ListName, $AppManifest)
}

<#
    .SYNOPSIS
    Reads a file from SharePoint, downloads it if necessary to the file system, and adds it to a given manifest.

	.DESCRIPTION
	Reads a file from SharePoint, downloads it if necessary to the file system, and adds it to a given manifest.

	Get-FileCreatorAndFolders uses the StorageType property to determine where to save the downloaded file.
	
	Valid values are defined by the [IQAppProvisioningBaseClasses.Provisioning.StorageTypes] enum and are [IQAppProvisioningBaseClasses.Provisioning.StorageTypes]::FileSystem (1) and [IQAppProvisioningBaseClasses.Provisioning.StorageTypes]::AzureStorage (2).

	If the value is FileSystem (1) you must set the AppManifestBase.BaseFilePath property to a valid path prior to using this command.

	If the value is AzureStorage (2) you must call the AppManifestBase.SetAzureStorageInfo method prior to using this command.

	Note that some file types, such as Wiki Pages and Publishing Pages are created from templates and properties. Therefore, the command updates the manifest, but no file is actually downloaded.

	If the file is stored in a document library that might not exist on the target site, you should include the list in the manifest using the Get-ListCreator command.
#>
function Get-FileCreatorAndFolders
{
	[CmdletBinding()]
	param
	(
	    [Parameter(Mandatory=$true, Position=1)]
	    [Microsoft.SharePoint.Client.ClientContext]$ClientContext,
		
		[Parameter(Mandatory=$true, Position=2)]
		[Microsoft.SharePoint.Client.Web]$Web,

		[Parameter(Mandatory=$true, Position=3)]
		[string]$FileWebRelativeUrl,

		[Parameter]
		[string]$DownloadFolderPath,
		
		[Parameter()]
	    [string]$AppManifestJSON,

		[Parameter()]
		[IQAppProvisioningBaseClasses.Provisioning.AppManifestBase]$AppManifest,

		[Parameter()]
		[bool]$GetRelatedFileCreators,

		[Parameter()]
		[switch]$VerboseNotify
	)
	$Error.Clear()
    $builder = New-Object IQAppManifestBuilders.FileCreatorBuilder

	if($VerboseNotify)
	{
		$builder.WriteNotificationsToStdOut = $true
	}
    
	if(($AppManifestJSON -ne $null) -or ($AppManifest -ne $null)){
		if($AppManifest -ne $null)
		{
			$manifest = $builder.GetFileCreator($ClientContext, $web, $FileWebRelativeUrl, $DownloadFolderPath, $AppManifest, $GetRelatedFileCreators)
		}
		else
		{
			$manifest = $builder.GetFileCreator($ClientContext, $web, $FileWebRelativeUrl, $DownloadFolderPath, $AppManifestJSON, $GetRelatedFileCreators)
		}
	}
	else
	{
		$manifest = $builder.GetFileCreator($ClientContext, $web, $FileWebRelativeUrl, $DownloadFolderPath)
	}
}

<#
    .SYNOPSIS
    Reads a RoleDefinition (aka Permission Level) from a SharePoint site and optionally adds it to a manifest.

	.DESCRIPTION
	Reads a RoleDefinition (aka Permission Level) from a SharePoint site and optionally adds it to a manifest. For more detail about the operation as it executes, include the -VerboseNotify switch.
#>
function Get-RoleDefinitionCreator
{
	[CmdletBinding()]
	param
	(
	    [Parameter(Mandatory=$true, Position=1)]
	    [Microsoft.SharePoint.Client.ClientContext]$ClientContext,
		
		[Parameter(Mandatory=$true, Position=2)]
		[Microsoft.SharePoint.Client.Web]$Web,

		[Parameter(Mandatory=$true, Position=3)]
		[string]$RoleDefinitionName,

	    [Parameter()]
	    [IQAppProvisioningBaseClasses.Provisioning.AppManifestBase]$AppManifest,

		[Parameter()]
		[switch]$VerboseNotify
	)
	$Error.Clear()
    $builder = New-Object IQAppManifestBuilders.CreatorBuilder
	$e = Register-ObjectEvent $builder VerboseNotify -Action {}
	if($VerboseNotify)
	{
		$builder.WriteNotificationsToStdOut = $true
	}
    $builder.GetCreator($ClientContext, $web, $RoleDefinitionName, $AppManifest, [IQAppManifestBuilders.CreatorTypes]::RoleDefinition)
}

<#
    .SYNOPSIS
    Reads a Group from a SharePoint site and optionally adds it to a manifest.

	.DESCRIPTION
	Reads a Group from a SharePoint site and optionally adds it to a manifest. For more detail about the operation as it executes, include the -VerboseNotify switch.
#>
function Get-GroupCreator
{
	[CmdletBinding()]
	param
	(
	    [Parameter(Mandatory=$true, Position=1)]
	    [Microsoft.SharePoint.Client.ClientContext]$ClientContext,
		
		[Parameter(Mandatory=$true, Position=2)]
		[Microsoft.SharePoint.Client.Web]$Web,

		[Parameter(Mandatory=$true, Position=3)]
		[string]$GroupName,

	    [Parameter()]
	    [IQAppProvisioningBaseClasses.Provisioning.AppManifestBase]$AppManifest,

		[Parameter()]
		[switch]$VerboseNotify
	)
	$Error.Clear()
    $builder = New-Object IQAppManifestBuilders.CreatorBuilder
	
	if($VerboseNotify)
	{
		$builder.WriteNotificationsToStdOut = $true
	}
    $builder.GetCreator($ClientContext, $web, $GroupName, $AppManifest, [IQAppManifestBuilders.CreatorTypes]::Group)
}

<#
    .SYNOPSIS
    Reads either all UserCustomActions or a single UserCustomAction by Title from a SharePoint site or web and optionally adds the result to a manifest.

	.DESCRIPTION
	Reads either all UserCustomActions or a single UserCustomAction by Title from a SharePoint site or web and optionally adds the result to a manifest.
	
	Note that custom actions associated with a list are included when you use the Get-ListCreator command.
	
	For more detail about the operation as it executes, include the -VerboseNotify switch.
#>
function Get-UserCustomActionCreators
{
	[CmdletBinding()]
	param
	(
	    [Parameter(Mandatory=$true, Position=1)]
	    [Microsoft.SharePoint.Client.ClientContext]$ClientContext,
		
		[Parameter()]
		[Microsoft.SharePoint.Client.Web]$Web,

		[Parameter()]
		[string]$CustomActionTitle,

	    [Parameter()]
	    [IQAppProvisioningBaseClasses.Provisioning.AppManifestBase]$AppManifest,

		[Parameter()]
		[bool]$SiteScope,

		[Parameter()]
		[switch]$VerboseNotify
	)
	$Error.Clear()
    $builder = New-Object IQAppManifestBuilders.CustomActionCreatorBuilder
	
	if($VerboseNotify)
	{
		$builder.WriteNotificationsToStdOut = $true
	}
	if($AppManifest -eq $null)
    {
        if($CustomActionTitle -eq "")
	    {
            $builder.GetCustomActionCreators($ClientContext, $Web, $SiteScope)
	    }
        else 
        {
            $builder.GetCustomActionCreator($ClientContext, $Web, $CustomActionTitle, $SiteScope)
        }
    }
    else
    {
        if($CustomActionTitle -eq "")
	    {
            $builder.GetCustomActionCreators($ClientContext, $Web, $AppManifest, $SiteScope)
	    }
        else 
        {
            $builder.GetCustomActionCreator($ClientContext, $Web, $CustomActionTitle, $AppManifest, $SiteScope)
        }
    }
}

<#
    .SYNOPSIS
    Reads the RemoteEventReceivers from a SharePoint site or web and optionally adds them to a manifest.

	.DESCRIPTION
	Reads the RemoteEventReceivers from a SharePoint site or web and optionally adds them to a manifest. 
	
	Note that remote event receivers associated with a list are included when you use the Get-ListCreator command.
	
	For more detail about the operation as it executes, include the -VerboseNotify switch.
#>
function Get-RemoteEventReceiverCreators
{
	[CmdletBinding()]
	param
	(
	    [Parameter(Mandatory=$true, Position=1)]
	    [Microsoft.SharePoint.Client.ClientContext]$ClientContext,
		
		[Parameter()]
		[Microsoft.SharePoint.Client.Web]$Web,

	    [Parameter()]
	    [IQAppProvisioningBaseClasses.Provisioning.AppManifestBase]$AppManifest,

		[Parameter()]
		[switch]$VerboseNotify
	)
	$Error.Clear()
    $builder = New-Object IQAppManifestBuilders.CreatorBuilder
	
	if($VerboseNotify)
	{
		$builder.WriteNotificationsToStdOut = $true
	}
    $builder.GetCreator($ClientContext, $web, "", $AppManifest, [IQAppManifestBuilders.CreatorTypes]::RemoteEvents)
}

<#
    .SYNOPSIS
    Reads the top or left navigation nodes from a SharePoint site or web and optionally adds them to a manifest.

	.DESCRIPTION
	Reads the top or left navigation nodes from a SharePoint site or web and optionally adds them to a manifest. 
	
	Valid values for NavigationCollection are 'Top' and 'Left'.
	
	For more detail about the operation as it executes, include the -VerboseNotify switch.
#>
function Get-NavigationCreator
{
	[CmdletBinding()]
	param
	(
	    [Parameter(Mandatory=$true, Position=1)]
	    [Microsoft.SharePoint.Client.ClientContext]$ClientContext,
		
		[Parameter()]
		[Microsoft.SharePoint.Client.Web]$Web,

		[Parameter()]
		[ValidateSet('Top','Left')]
		[string]$NavigationCollection,

	    [Parameter()]
	    [IQAppProvisioningBaseClasses.Provisioning.AppManifestBase]$AppManifest,

		[Parameter()]
		[switch]$VerboseNotify
	)
	$Error.Clear()
    $builder = New-Object IQAppManifestBuilders.CreatorBuilder
	
	if($VerboseNotify)
	{
		$builder.WriteNotificationsToStdOut = $true
	}
    $builder.GetCreator($ClientContext, $web, $NavigationCollection, $AppManifest, [IQAppManifestBuilders.CreatorTypes]::Navigation)
}

<#
    .SYNOPSIS
    Reads the branding information from a SharePoint site or web and optionally adds it to a manifest.

	.DESCRIPTION
	Reads the branding information from a SharePoint site or web and optionally adds it to a manifest.

	Values include :
		Site Title
		Site Logo Url
		AlternateCssUrl
		DefaultMasterPageUrl
		CustomMasterPageUrl
		The current composed look

	Note that this command does not read the actual files associated with these values. To include the files use the Get-FileCreatorAndFolders command for each file as necessary.
	
	For more detail about the operation as it executes, include the -VerboseNotify switch.
#>
function Get-LookAndFeelCreator
{
	[CmdletBinding()]
	param
	(
	    [Parameter(Mandatory=$true, Position=1)]
	    [Microsoft.SharePoint.Client.ClientContext]$ClientContext,
		
		[Parameter()]
		[Microsoft.SharePoint.Client.Web]$Web,

	    [Parameter()]
		[IQAppProvisioningBaseClasses.Provisioning.AppManifestBase]$AppManifest,

		[Parameter()]
		[switch]$VerboseNotify
	)
	$Error.Clear()
    $builder = New-Object IQAppManifestBuilders.CreatorBuilder
	
    if($VerboseNotify)
	{
		$builder.WriteNotificationsToStdOut = $true
	}
	$builder.GetCreator($ClientContext, $Web, $null, $AppManifest, [IQAppManifestBuilders.CreatorTypes]::LookAndFeel)
}

<#
    .SYNOPSIS
    Populates or updates a web definition by comparing a site with customizations to a base site

	.DESCRIPTION
	Populates or updates a web creator in a site definition or an app manifest by comparing a site with customizations to a base site.
	
	If this operation is done with a site defintion it will also create or update the first app manifest.
	
	Get-WebCreator will download files using the storage information included in the site definition or app manifest. 
	
	For more detail about the operation as it executes, include the -VerboseNotify switch.
#>
function Get-WebCreator
{
	[CmdletBinding()]
	param
	(
	    [Parameter(Mandatory=$true, Position=1)]
	    [Microsoft.SharePoint.Client.ClientContext]$SourceClientContext,

		[Parameter(Mandatory=$true, Position=2)]
	    [Microsoft.SharePoint.Client.ClientContext]$BaseClientContext,

		[Parameter()]
		[IQAppProvisioningBaseClasses.Provisioning.SiteDefinition]$SiteDefinition,

	    [Parameter()]
	    [IQAppProvisioningBaseClasses.Provisioning.WebCreator]$WebDefinition,

		[Parameter()]
	    [IQAppProvisioningBaseClasses.Provisioning.AppManifestBase]$AppManifest,

		[Parameter()]
	    [IQAppManifestBuilders.WebCreatorBuilderOptions]$Options,

		[Parameter()]
		[switch]$VerboseNotify
	)
	$Error.Clear()
	if($SiteDefinition -eq $null -and $AppManifest -eq $null)
	{
		Write-Error "Please include either a site definition or app manifest."
	}
	else
	{
		$builder = New-Object IQAppManifestBuilders.WebCreatorBuilder
	
		if($VerboseNotify)
		{
			$builder.WriteNotificationsToStdOut = $true
		}
		if($SiteDefinition -ne $null)
		{
			$builder.GetWebCreatorBuilder($SiteDefinition, $WebDefinition, $Options, $SourceClientContext, $BaseClientContext)
		}
		else 
		{
			$builder.GetWebCreatorBuilder($AppManifest, $Options, $SourceClientContext, $BaseClientContext)
		}
	}
}

<#
    .SYNOPSIS
    Converts a file system based manifest to azure storage and copies files to the specified container.

	.DESCRIPTION
	Converts a file system based manifest to azure storage and copies files to the specified container.
	
	For more detail about the operation as it executes, include the -VerboseNotify switch.
#>
function ConvertTo-BlobStorageIQApp
{
	[CmdletBinding()]
	param
	(
	    [Parameter()]
	    [IQAppProvisioningBaseClasses.Provisioning.AppManifestBase]$AppManifest,

		[Parameter()]
	    [string]$StorageAccount,

		[Parameter()]
	    [string]$AccountKey,

		[Parameter()]
		[switch]$VerboseNotify
	)
	$Error.Clear()
    $migrator = New-Object IQAppStorageMigrator.Migrator
	
	if($VerboseNotify)
	{
		$builder.WriteNotificationsToStdOut = $true
	}
    $migrator.MigrateFromFileSystemToAzure($AppManifest, $StorageAccount, $AccountKey)
}

<#
    .SYNOPSIS
    Creates an instance of a SiteDefinition object from scratch or from JSON

	.DESCRIPTION
	New-IQAppManifest returns an object of type IQAppProvisioningBaseClasses.Provisioning.SiteDefinition as a new object or from JSON.

	The JSON can be in the form of a string or a file located in a file system volume or Azure Storage.
	
	If the file is located in Azure storage, it must be named sitedefinition.json. 
#>
function New-IQSiteDefinition
{
	[CmdletBinding()]
	param
	(
	    [Parameter()]
	    [string]$JSON,
		
		[Parameter()]
		[string]$JSONFilePath,

		[Parameter()]
	    [bool]$PreserveBaseFilePath,
				
		[Parameter()]
	    [string]$StorageAccount,

		[Parameter()]
	    [string]$AccountKey,

		[Parameter()]
	    [string]$Container
	)
	$Error.Clear()

	if(($JSON -eq "") -and ($JSONFilePath -eq "") -and ($StorageAccount -eq "")) 
	{
		$siteDefinition = New-Object IQAppProvisioningBaseClasses.Provisioning.SiteDefinition
		$siteDefinition.StorageType = [IQAppProvisioningBaseClasses.Provisioning.StorageTypes]::FileSystem
		return $siteDefinition
	}
	elseif ($JSON -ne "")
	{
		return [IQAppProvisioningBaseClasses.Provisioning.SiteDefinition]::GetSiteDefinitionFromJSON($JSON)
	}
	elseif ($JSONFilePath -ne "")
	{	
		Write-Host $JSONFilePath
		$j = Get-Content -Path "$JSONFilePath" -Encoding UTF8
		$siteDefinition = [IQAppProvisioningBaseClasses.Provisioning.SiteDefinition]::GetSiteDefinitionFromJSON($j)
		if($PreserveBaseFilePath -ne $true)
		{
			$siteDefinition.BaseFilePath = Split-Path -Path $JSONFilePath -Parent
		}
		return $siteDefinition
	}
	elseif (($StorageAccount -ne "") -and ($AccountKey -ne "") -and ($Container -eq ""))
	{	
		$siteDefinition = New-Object IQAppProvisioningBaseClasses.Provisioning.SiteDefinition
		$siteDefinition.StorageType = [IQAppProvisioningBaseClasses.Provisioning.StorageTypes]::AzureStorage
		$newContainer = ($siteDefinition.SiteDefinitionId + "-" + $siteDefinition.Version.ToString()).Replace(".", "-");
		$siteDefinition.SetAzureStorageInfo($StorageAccount, $AccountKey, $newContainer)
		Save-IQSiteDefinition $siteDefinition
		return $siteDefinition
	}
	else
	{
		return [IQAppProvisioningBaseClasses.Provisioning.SiteDefinition]::GetSiteDefinitionFromAzureStorage($StorageAccount, $AccountKey, $Container)
	}
}

<#
    .SYNOPSIS
    Creates an instance of a WebCreator object for inclusion in a site definition

	.DESCRIPTION
	New-WebCreator returns an object of type IQAppProvisioningBaseClasses.Provisioning.WebCreator as a new object.
#>
function New-WebCreator 
{
	return New-Object IQAppProvisioningBaseClasses.Provisioning.WebCreator
}

<#
    .SYNOPSIS
    Saves an [IQAppProvisioningBaseClasses.Provisioning.SiteDefinition] instance as serialized JSON to disk or to Azure storage.

	.DESCRIPTION
	Save-IQSiteDefinition uses the StorageType property to determine how to save the manifest.
	
	Valid values are defined by the [IQAppProvisioningBaseClasses.Provisioning.StorageTypes] enum and are [IQAppProvisioningBaseClasses.Provisioning.StorageTypes]::FileSystem (1) and [IQAppProvisioningBaseClasses.Provisioning.StorageTypes]::AzureStorage (2).

	If the value is FileSystem (1) you must set the SiteDefinition.BaseFilePath property to a valid path prior to using this command.

	If the value is AzureStorage (2) you must call the SiteDefinition.SetAzureStorageInfo method prior to using this command. 
	
	The generated file will be named sitedefinition.json. If a sitedefinition.json already exists at the specified location it will be overwritten.
#>
function Save-IQSiteDefinition
{
	[CmdletBinding()]
	param
	(
	    [Parameter()]
	    [IQAppProvisioningBaseClasses.Provisioning.SiteDefinition]$SiteDefinition
	)

	$Error.Clear()
	if($SiteDefinition.GetAzureStorageInfo() -ne $null){
		return [IQAppProvisioningBaseClasses.Provisioning.SiteDefinition]::SaveSiteDefinitionToAzureStorage($SiteDefinition)
	}
	else {
		return [IQAppProvisioningBaseClasses.Provisioning.SiteDefinition]::SaveSiteDefinitionToFileSystem($SiteDefinition)
	}
}

<#
    .SYNOPSIS
    Configures a pre-existing SharePoint site, provisions zero or more manifests, and creates zero or more subsites also with optional manifests.

	.DESCRIPTION
	Configures a pre-existing SharePoint site, provisions zero or more manifests, and creates zero or more subsites also with optional manifests.

	The site definition can either be provided using an existing instance of IQAppProvisioningBaseClasses.Provisioning.SiteDefinition or be read from a file. 

	If provisioning from Azure Storage, you must first load the site definition using the New-IQSiteDefinition command.
#>
function Install-IQSiteDefinition
{
	[CmdletBinding()]
	param
	(
		#This should be a client context of the pre-existing web to which the site definition is installed
		[Parameter(Mandatory=$true, Position=1)]
	    [Microsoft.SharePoint.Client.ClientContext]$ClientContext,
		
		[Parameter(Mandatory=$true, Position=2)]
		[Microsoft.SharePoint.Client.Web]$Web,

	    [Parameter(Position=3)]
	    [string]$AbsoluteJSONPath,

	    [Parameter()]
	    [IQAppProvisioningBaseClasses.Provisioning.SiteDefinition]$SiteDefinition,

		[Parameter()]
		[switch]$VerboseNotify
	)
	$Error.Clear()
    
	$provisioner = New-Object IQAppSiteProvisioner.Provisioner
    
	if($VerboseNotify)
	{
		$provisioner.WriteNotificationsToStdOut = $true
	}
	if($SiteDefinition -eq $null)
	{
		Write-Host $provisioner.Provision($ClientContext, $Web, $AbsoluteJSONPath)
	}
	else
	{
		Write-Host $provisioner.Provision($ClientContext, $Web, $SiteDefinition)
	}
}

<#
    .SYNOPSIS
    Removes items including subsites from a SharePoint site based on the contents of the site definition.

	.DESCRIPTION
	Removes items including subsites from a SharePoint site based on the contents of the site definition.

	Note that this operation is destructive and should be used with extreme caution.

	Files that are not part of a document library or sub-site are only removed if the FileCreator.DeleteOnCleanup property is set to true.

	All content in lists, libraries, and sub-sites included in the site definition is deleted.
#>
function Uninstall-IQSiteDefinition
{
	[CmdletBinding()]
	param
	(
	    [Parameter(Mandatory=$true, Position=1)]
	    [Microsoft.SharePoint.Client.ClientContext]$ClientContext,
		
		[Parameter(Mandatory=$true, Position=2)]
		[Microsoft.SharePoint.Client.Web]$Web,

	    [Parameter(Position=3)]
	    [string]$AbsoluteJSONPath,

		[Parameter()]
	    [IQAppProvisioningBaseClasses.Provisioning.SiteDefinition]$SiteDefinition,

		[Parameter()]
		[switch]$VerboseNotify,

		[Parameter()]
	    [switch]$Force
	)
	$Error.Clear()

	if($Force -ne $true)
	{
		$message  = 'DANGER!!!'
		$question = 'This will remove the any child webs defined by the site definition and all content in those webs. Are you sure you want to proceed?'

		$choices = New-Object Collections.ObjectModel.Collection[Management.Automation.Host.ChoiceDescription]
		$choices.Add((New-Object Management.Automation.Host.ChoiceDescription -ArgumentList '&Yes'))
		$choices.Add((New-Object Management.Automation.Host.ChoiceDescription -ArgumentList '&No'))

		$decision = $Host.UI.PromptForChoice($message, $question, $choices, 1)
	}

	if ($Force -eq $true -or $decision -eq 0) {
		$provisioner = New-Object IQAppSiteProvisioner.Provisioner
		if($VerboseNotify)
		{
			$provisioner.WriteNotificationsToStdOut = $true
		}
		if($SiteDefinition -eq $null)
		{
			Write-Host $provisioner.Deprovision($ClientContext, $Web, $AbsoluteJSONPath)
		}
		else
		{
			Write-Host $provisioner.Deprovision($ClientContext, $Web, $SiteDefinition)
		}
	} 
}