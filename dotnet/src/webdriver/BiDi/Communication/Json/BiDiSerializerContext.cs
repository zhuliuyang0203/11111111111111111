using System.Text.Json.Serialization;

namespace OpenQA.Selenium.BiDi.Communication.Json;

// https://github.com/dotnet/runtime/issues/72604
[JsonSerializable(typeof(MessageSuccess))]
[JsonSerializable(typeof(MessageError))]
[JsonSerializable(typeof(MessageEvent))]

[JsonSerializable(typeof(Modules.Script.EvaluateResult.Success))]
[JsonSerializable(typeof(Modules.Script.EvaluateResult.Exception))]

[JsonSerializable(typeof(Modules.Script.RemoteValue.Number), TypeInfoPropertyName = "Script_RemoteValue_Number")]
[JsonSerializable(typeof(Modules.Script.RemoteValue.String), TypeInfoPropertyName = "Script_RemoteValue_String")]
[JsonSerializable(typeof(Modules.Script.RemoteValue.Null), TypeInfoPropertyName = "Script_RemoteValue_Null")]
[JsonSerializable(typeof(Modules.Script.RemoteValue.Undefined), TypeInfoPropertyName = "Script_RemoteValue_Undefined")]
[JsonSerializable(typeof(Modules.Script.RemoteValue.Symbol))]
[JsonSerializable(typeof(Modules.Script.RemoteValue.Object), TypeInfoPropertyName = "Script_RemoteValue_Object")]
[JsonSerializable(typeof(Modules.Script.RemoteValue.Function))]
[JsonSerializable(typeof(Modules.Script.RemoteValue.RegExp), TypeInfoPropertyName = "Script_RemoteValue_RegExp")]
[JsonSerializable(typeof(Modules.Script.RemoteValue.RegExp.RegExpValue), TypeInfoPropertyName = "Script_RemoteValue_RegExp_RegExpValue")]
[JsonSerializable(typeof(Modules.Script.RemoteValue.Date), TypeInfoPropertyName = "Script_RemoteValue_Date")]
[JsonSerializable(typeof(Modules.Script.RemoteValue.Map), TypeInfoPropertyName = "Script_RemoteValue_Map")]
[JsonSerializable(typeof(Modules.Script.RemoteValue.Set), TypeInfoPropertyName = "Script_RemoteValue_Set")]
[JsonSerializable(typeof(Modules.Script.RemoteValue.WeakMap))]
[JsonSerializable(typeof(Modules.Script.RemoteValue.WeakSet))]
[JsonSerializable(typeof(Modules.Script.RemoteValue.Generator))]
[JsonSerializable(typeof(Modules.Script.RemoteValue.Error))]
[JsonSerializable(typeof(Modules.Script.RemoteValue.Proxy))]
[JsonSerializable(typeof(Modules.Script.RemoteValue.Promise))]
[JsonSerializable(typeof(Modules.Script.RemoteValue.TypedArray))]
[JsonSerializable(typeof(Modules.Script.RemoteValue.ArrayBuffer))]
[JsonSerializable(typeof(Modules.Script.RemoteValue.NodeList))]
[JsonSerializable(typeof(Modules.Script.RemoteValue.HtmlCollection))]
[JsonSerializable(typeof(Modules.Script.RemoteValue.Node))]
[JsonSerializable(typeof(Modules.Script.RemoteValue.WindowProxy))]

[JsonSerializable(typeof(Modules.Script.LocalValue.String), TypeInfoPropertyName = "Script_LocalValue_String")]

[JsonSerializable(typeof(Modules.Script.Target.Realm), TypeInfoPropertyName = "Script_Target_Realm")]
[JsonSerializable(typeof(Modules.Script.Target.Context), TypeInfoPropertyName = "Script_Target_Context")]

[JsonSerializable(typeof(Modules.Script.RealmInfo.Window))]
[JsonSerializable(typeof(Modules.Script.RealmInfo.DedicatedWorker))]
[JsonSerializable(typeof(Modules.Script.RealmInfo.SharedWorker))]
[JsonSerializable(typeof(Modules.Script.RealmInfo.ServiceWorker))]
[JsonSerializable(typeof(Modules.Script.RealmInfo.Worker))]
[JsonSerializable(typeof(Modules.Script.RealmInfo.PaintWorklet))]
[JsonSerializable(typeof(Modules.Script.RealmInfo.AudioWorklet))]
[JsonSerializable(typeof(Modules.Script.RealmInfo.Worklet))]

[JsonSerializable(typeof(Modules.Log.Entry.Console))]
[JsonSerializable(typeof(Modules.Log.Entry.Javascript))]
//

[JsonSerializable(typeof(Command))]
[JsonSerializable(typeof(Message))]

[JsonSerializable(typeof(Modules.Session.StatusResult))]
[JsonSerializable(typeof(Modules.Session.NewResult))]

[JsonSerializable(typeof(Modules.Browser.CloseCommand), TypeInfoPropertyName = "Browser_CloseCommand")]
[JsonSerializable(typeof(Modules.Browser.UserContextInfo))]
[JsonSerializable(typeof(Modules.Browser.GetUserContextsResult))]

[JsonSerializable(typeof(Modules.BrowsingContext.CloseCommand), TypeInfoPropertyName = "BrowsingContext_CloseCommand")]
[JsonSerializable(typeof(Modules.BrowsingContext.CreateResult))]
[JsonSerializable(typeof(Modules.BrowsingContext.BrowsingContextInfo))]
[JsonSerializable(typeof(Modules.BrowsingContext.NavigateResult))]
[JsonSerializable(typeof(Modules.BrowsingContext.NavigationInfo))]
[JsonSerializable(typeof(Modules.BrowsingContext.TraverseHistoryResult))]
[JsonSerializable(typeof(Modules.BrowsingContext.LocateNodesResult))]
[JsonSerializable(typeof(Modules.BrowsingContext.CaptureScreenshotResult))]
[JsonSerializable(typeof(Modules.BrowsingContext.GetTreeResult))]
[JsonSerializable(typeof(Modules.BrowsingContext.PrintResult))]
[JsonSerializable(typeof(Modules.BrowsingContext.UserPromptOpenedEventArgs))]
[JsonSerializable(typeof(Modules.BrowsingContext.UserPromptClosedEventArgs))]

[JsonSerializable(typeof(Modules.Network.BytesValue.String), TypeInfoPropertyName = "Network_BytesValue_String")]
[JsonSerializable(typeof(Modules.Network.UrlPattern.String), TypeInfoPropertyName = "Network_UrlPattern_String")]
[JsonSerializable(typeof(Modules.Network.ContinueWithAuthParameters.Default), TypeInfoPropertyName = "Network_ContinueWithAuthParameters_Default")]
[JsonSerializable(typeof(Modules.Network.AddInterceptResult))]
[JsonSerializable(typeof(Modules.Network.BeforeRequestSentEventArgs))]
[JsonSerializable(typeof(Modules.Network.ResponseStartedEventArgs))]
[JsonSerializable(typeof(Modules.Network.ResponseCompletedEventArgs))]
[JsonSerializable(typeof(Modules.Network.FetchErrorEventArgs))]
[JsonSerializable(typeof(Modules.Network.AuthRequiredEventArgs))]

[JsonSerializable(typeof(Modules.Script.Channel), TypeInfoPropertyName = "Script_Channel")]
[JsonSerializable(typeof(Modules.Script.AddPreloadScriptResult))]
[JsonSerializable(typeof(Modules.Script.EvaluateResult))]
[JsonSerializable(typeof(Modules.Script.GetRealmsResult))]

[JsonSerializable(typeof(Modules.Log.Entry))]

[JsonSerializable(typeof(Modules.Storage.GetCookiesResult))]
[JsonSerializable(typeof(Modules.Storage.DeleteCookiesResult))]
[JsonSerializable(typeof(Modules.Storage.SetCookieResult))]
internal partial class BiDiSerializerContext: JsonSerializerContext;
