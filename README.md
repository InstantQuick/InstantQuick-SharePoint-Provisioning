# SProvisioner
SProvisioner is the easiest to use and most complete SharePoint provisioning library currently available. It is the core engine used in the InstantQuick line of products and can read from and provision to SharePoint 2013, SharePoint 2016, and SharePoint Online.

This repository includes the .NET class libraries we use at InstantQuick and a companion PowerShell module.

This codebase predates Office PnPCore by a couple of years and differs in that it is designed to be turnkey and easy to use with minimal setup. It also offers more features out of the box including the ability to read and provion Web Part Pages, Wiki Pages,  Publishing Pages, and 2010 style workflows against versions of SharePoint that do not support the latest SharePoint Client Object Model API's by falling back to older API's as needed.

As with the Microsoft Patterns and Practices library, SProvisioner can generate templates by comparing a customized site to a base site. Unlike the PnP engine you can easily include any file in the site (including publishing pages and page layouts) without writing code or being forced to extend the library. It also has the capabilty to provision site heirarchies and to both install and/or remove multiple template manifests as a single operation. 

##Features
SProvisioner can read and recreate the following out of the box
+ Webs and subwebs
+ Fields
+ Content Types
+ Lists and Libraries with or without custom views
+ List items
+ Documents
+ Folders
+ Web Part Pages
+ Wiki Pages
+ Publishing Pages
+ Master Pages
+ Page layouts
+ Display templates
+ Composed looks and themes
+ Other arbitrary file types with or without document properties
+ Feature activation and deactivation
+ Permission levels
+ Groups
+ Role assignments (item permissions)
+ Top and left navigation
+ Document templates
+ 2010 Workflows
+ 2013 Workflows
+ Managed metadata
+ Site, Web, and List custom actions
+ AppWeb navigation surfaces
+ ...and more
 
This page is a preview and the repository is currently empty, but not for long!

