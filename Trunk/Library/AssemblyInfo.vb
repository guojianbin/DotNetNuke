'
' DotNetNuke - http://www.dotnetnuke.com
' Copyright (c) 2002-2010
' by DotNetNuke Corporation
'
' Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated 
' documentation files (the "Software"), to deal in the Software without restriction, including without limitation 
' the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and 
' to permit persons to whom the Software is furnished to do so, subject to the following conditions:
'
' The above copyright notice and this permission notice shall be included in all copies or substantial portions 
' of the Software.
'
' THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED 
' TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL 
' THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF 
' CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER 
' DEALINGS IN THE SOFTWARE.
'

Imports System.Reflection
Imports System.Runtime.InteropServices
Imports DotNetNuke.Application
Imports System.Runtime.CompilerServices

' General Information about an assembly is controlled through the following 
' set of attributes. Change these attribute values to modify the information
' associated with an assembly.

' Review the values of the assembly attributes

<Assembly: AssemblyTitle("DotNetNuke")> 
<Assembly: AssemblyDescription("Open Source Web Application Framework")> 
<Assembly: AssemblyCompany("DotNetNuke Corporation")> 
<Assembly: AssemblyProduct("http://www.dotnetnuke.com")> 
<Assembly: AssemblyCopyright("DotNetNuke is copyright 2002-2008 by DotNetNuke Corporation. All Rights Reserved.")> 
<Assembly: AssemblyTrademark("DotNetNuke")> 
<Assembly: CLSCompliant(True)> 

' Version information for an assembly consists of the following four values:
'
'      Major Version
'      Minor Version 
'      Build Number
'      Revision
'
' You can specify all the values or you can default the Build and Revision Numbers 
' by using the '*' as shown below:

<Assembly: AssemblyVersion("5.4.0.38")> 
<Assembly: AssemblyStatus(ReleaseMode.Alpha)> 

'Allow internal variables to be visible to testing projects
<Assembly: InternalsVisibleTo("DotNetNuke.Tests")> 
