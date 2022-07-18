﻿using System;
using System.Reflection;
using System.Runtime.InteropServices;
using MelonLoader;
using Serpent.Loader;

// General Information about an assembly is controlled through the following
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
[assembly: AssemblyTitle(Serpent.Loader.BuildInfo.Name)]
[assembly: AssemblyDescription("")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany(Serpent.Loader.BuildInfo.Company)]
[assembly: AssemblyProduct(Serpent.Loader.BuildInfo.Name)]
[assembly: AssemblyCopyright("Created by " + Serpent.Loader.BuildInfo.Author)]
[assembly: AssemblyTrademark(Serpent.Loader.BuildInfo.Company)]
[assembly: AssemblyCulture("")]

// Setting ComVisible to false makes the types in this assembly not visible
// to COM components.  If you need to access a type in this assembly from
// COM, set the ComVisible attribute to true on that type.
[assembly: ComVisible(false)]

// The following GUID is for the ID of the typelib if this project is exposed to COM
[assembly: Guid("6e5e0e59-73d9-4d74-b734-d040d26710c4")]

// Version information for an assembly consists of the following four values:
//
//      Major Version
//      Minor Version
//      Build Number
//      Revision
//
// You can specify all the values or you can default the Build and Revision Numbers
// by using the '*' as shown below:
// [assembly: AssemblyVersion("1.0.*")]
[assembly: AssemblyVersion(Serpent.Loader.BuildInfo.Version)]
[assembly: AssemblyFileVersion(Serpent.Loader.BuildInfo.Version)]

[assembly: MelonInfo(typeof(ReLoader), Serpent.Loader.BuildInfo.Name, Serpent.Loader.BuildInfo.Version, Serpent.Loader.BuildInfo.Author, Serpent.Loader.BuildInfo.DownloadLink)]

// Create and Setup a MelonModGame to mark a Mod as Universal or Compatible with specific Games.
// If no MelonModGameAttribute is found or any of the Values for any MelonModGame on the Mod is null or empty it will be assumed the Mod is Universal.
// Values for MelonModGame can be found in the Game's app.info file or printed at the top of every log directly beneath the Unity version.
[assembly: MelonGame("VRChat", "VRChat")]
[assembly: MelonColor(ConsoleColor.Magenta)]