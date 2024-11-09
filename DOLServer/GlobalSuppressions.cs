using System.Diagnostics.CodeAnalysis;

[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "DOL", Scope = "namespace", Target = "DOL.DOLServer")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "DOL", Scope = "namespace", Target = "DOL.DOLServer.Actions")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "DOL", Scope = "namespace", Target = "DOLGameServerConsole")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA2210:AssembliesShouldHaveValidStrongNames")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "DOL")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1014:MarkAssembliesWithClsCompliant")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1017:MarkAssembliesWithComVisible")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1824:MarkAssembliesWithNeutralResourcesLanguage")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1304:SpecifyCultureInfo", MessageId = "System.String.ToLower", Scope = "member", Target = "DOL.DOLServer.Actions.ConsoleStart.#OnAction(System.Collections.Hashtable)")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Scope = "member", Target = "DOL.DOLServer.Actions.ConsoleStart.#OnAction(System.Collections.Hashtable)")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Scope = "member", Target = "DOL.DOLServer.Actions.ServiceInstall.#OnAction(System.Collections.Hashtable)")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Scope = "member", Target = "DOL.DOLServer.Actions.ServiceUninstall.#OnAction(System.Collections.Hashtable)")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1305:SpecifyIFormatProvider", MessageId = "System.String.Format(System.String,System.Object,System.Object,System.Object)", Scope = "member", Target = "DOLGameServerConsole.ConsolePacketLib.#SendMessage(System.String,DOL.GS.PacketHandler.eChatType,DOL.GS.PacketHandler.eChatLoc)")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1305:SpecifyIFormatProvider", MessageId = "System.String.Format(System.String,System.Object)", Scope = "member", Target = "DOLGameServerConsole.ConsolePacketLib.#SendCustomDialog(System.String,DOL.GS.PacketHandler.CustomDialogResponse)")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1305:SpecifyIFormatProvider", MessageId = "System.String.Format(System.String,System.Object,System.Object,System.Object)", Scope = "member", Target = "DOLGameServerConsole.ConsolePacketLib.#SendCustomDialog(System.String,DOL.GS.PacketHandler.CustomDialogResponse)")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1725:ParameterNamesShouldMatchBaseDeclaration", MessageId = "6#", Scope = "member", Target = "DOLGameServerConsole.ConsolePacketLib.#SendDialogBox(DOL.GS.PacketHandler.eDialogCode,System.UInt16,System.UInt16,System.UInt16,System.UInt16,DOL.GS.PacketHandler.eDialogType,System.Boolean,System.String)")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Scope = "member", Target = "DOLGameServerConsole.ConsolePacketLib.#SendInventoryItemsPartialUpdate(System.Collections.Generic.IDictionary`2<System.Int32,DOL.Database.InventoryItem>,DOL.GS.PacketHandler.eInventoryWindowType)")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "items", Scope = "member", Target = "DOLGameServerConsole.ConsolePacketLib.#SendInventoryItemsPartialUpdate(System.Collections.Generic.IDictionary`2<System.Int32,DOL.Database.InventoryItem>,DOL.GS.PacketHandler.eInventoryWindowType)")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "windowType", Scope = "member", Target = "DOLGameServerConsole.ConsolePacketLib.#SendInventoryItemsPartialUpdate(System.Collections.Generic.IDictionary`2<System.Int32,DOL.Database.InventoryItem>,DOL.GS.PacketHandler.eInventoryWindowType)")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1725:ParameterNamesShouldMatchBaseDeclaration", MessageId = "1#", Scope = "member", Target = "DOLGameServerConsole.ConsolePacketLib.#SendKeepComponentUpdate(DOL.GS.Keeps.AbstractGameKeep,System.Boolean)")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Scope = "member", Target = "DOLGameServerConsole.ConsolePacketLib.#SendKeepDoorUpdate(DOL.GS.Keeps.GameKeepDoor)")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "door", Scope = "member", Target = "DOLGameServerConsole.ConsolePacketLib.#SendKeepDoorUpdate(DOL.GS.Keeps.GameKeepDoor)")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Scope = "member", Target = "DOLGameServerConsole.ConsolePacketLib.#SendHousePermissions(DOL.GS.Housing.House)")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "house", Scope = "member", Target = "DOLGameServerConsole.ConsolePacketLib.#SendHousePermissions(DOL.GS.Housing.House)")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1026:DefaultParametersShouldNotBeUsed", Scope = "member", Target = "DOLGameServerConsole.ConsolePacketLib.#SendExitHouse(DOL.GS.Housing.House,System.UInt16)")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1002:DoNotExposeGenericLists", Scope = "member", Target = "DOLGameServerConsole.ConsolePacketLib.#SendWarmapDetailUpdate(System.Collections.Generic.List`1<System.Collections.Generic.List`1<System.Byte>>,System.Collections.Generic.List`1<System.Collections.Generic.List`1<System.Byte>>)")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2201:DoNotRaiseReservedExceptionTypes", Scope = "member", Target = "DOL.DOLServer.GameServerService.#OnStart(System.String[])")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "DOL", Scope = "member", Target = "DOL.DOLServer.GameServerService.#GetDOLService()")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate", Scope = "member", Target = "DOL.DOLServer.GameServerService.#GetDOLService()")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1304:SpecifyCultureInfo", MessageId = "System.String.ToLower", Scope = "member", Target = "DOL.DOLServer.GameServerService.#GetDOLService()")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1504:ReviewMisleadingFieldNames", Scope = "member", Target = "DOL.DOLServer.MainClass.#m_actions")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1305:SpecifyIFormatProvider", MessageId = "System.String.Format(System.String,System.Object,System.Object)", Scope = "member", Target = "DOL.DOLServer.MainClass.#ShowSyntax()")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1307:SpecifyStringComparison", MessageId = "System.String.StartsWith(System.String)", Scope = "member", Target = "DOL.DOLServer.MainClass.#ParseParameters(System.String[],System.String&,System.Collections.Hashtable&)")]
[assembly: SuppressMessage("Wrong Usage", "DF0001:Marks undisposed anonymous objects from method invocations.", Justification = "<Ausstehend>", Scope = "member", Target = "~M:DOL.DOLServer.Actions.ConsoleStart.OnAction(System.Collections.Hashtable)")]
[assembly: SuppressMessage("Wrong Usage", "DF0010:Marks undisposed local variables.", Justification = "<Ausstehend>", Scope = "member", Target = "~M:DOL.DOLServer.Actions.ServiceInstall.OnAction(System.Collections.Hashtable)")]
[assembly: SuppressMessage("Wrong Usage", "DF0001:Marks undisposed anonymous objects from method invocations.", Justification = "<Ausstehend>", Scope = "member", Target = "~M:DOL.DOLServer.Actions.ServiceInstall.OnAction(System.Collections.Hashtable)")]
[assembly: SuppressMessage("Wrong Usage", "DF0010:Marks undisposed local variables.", Justification = "<Ausstehend>", Scope = "member", Target = "~M:DOL.DOLServer.Actions.ServiceUninstall.OnAction(System.Collections.Hashtable)")]
[assembly: SuppressMessage("Wrong Usage", "DF0001:Marks undisposed anonymous objects from method invocations.", Justification = "<Ausstehend>", Scope = "member", Target = "~M:DOL.DOLServer.Actions.ServiceUninstall.OnAction(System.Collections.Hashtable)")]
// Diese Datei wird von Code Analysis zur Wartung der SuppressMessage- 
// Attribute verwendet, die auf dieses Projekt angewendet werden.
// Unterdrückungen auf Projektebene haben entweder kein Ziel oder 
// erhalten ein spezifisches Ziel mit Namespace-, Typ-, Memberbereich usw.
//
// Wenn Sie dieser Datei eine Unterdrückung hinzufügen möchten, klicken Sie mit der rechten Maustaste auf die Meldung in den 
// Code Analysis-Ergebnissen, zeigen Sie auf "Meldung unterdrücken", und klicken Sie auf 
// "In Unterdrückungsdatei".
// Sie müssen dieser Datei nicht manuell Unterdrückungen hinzufügen.

