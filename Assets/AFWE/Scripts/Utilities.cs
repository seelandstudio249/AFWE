//using System;
//using System.Collections;
//using System.Collections.Generic;
//using System.IO;
//using UnityEngine;

//public static class DebugLogger {
//	private static readonly string logFilePath = Path.Combine(Application.persistentDataPath, "app_logs.txt");

//	public static void Log(string message) {
//		if (string.IsNullOrEmpty(message)) {
//			message = "Empty or null message logged.";
//		}
//		string formattedMessage = FormatLogMessage("INFO", message);
//		WriteToLog(formattedMessage);
//	}

//	public static void LogWarning(string message) {
//		if (string.IsNullOrEmpty(message)) {
//			message = "Empty or null warning logged.";
//		}
//		string formattedMessage = FormatLogMessage("WARNING", message);
//		WriteToLog(formattedMessage);
//	}

//	//public static void LogError(string message, Exception ex = null) {
//	//	if (string.IsNullOrEmpty(message)) {
//	//		message = "Empty or null error logged.";
//	//	}

//	//	string formattedMessage = FormatLogMessage("ERROR", message);

//	//	var stackTrace = new System.Diagnostics.StackTrace();
//	//	string callerMethod = stackTrace.GetFrame(1)?.GetMethod()?.Name ?? "Unknown Method";

//	//	formattedMessage += $"\nCaller Method: {callerMethod}";
//	//	formattedMessage += $"\nStackTrace: {(ex != null ? ex.StackTrace : Environment.StackTrace)}";

//	//	WriteToLog(formattedMessage);
//	//}

//	//public static void LogError(string message, Exception ex = null) {
//	//	if (string.IsNullOrEmpty(message)) {
//	//		message = "Empty or null error logged.";
//	//	}

//	//	var stackTrace = new System.Diagnostics.StackTrace();
//	//	var frame = stackTrace.GetFrame(1); // Get the caller method
//	//	string callerMethod = frame?.GetMethod()?.Name ?? "Unknown Method";
//	//	string callerClass = frame?.GetMethod()?.DeclaringType?.Name ?? "Unknown Class";

//	//	string formattedMessage = FormatLogMessage("ERROR", message);
//	//	formattedMessage += $"\nCaller Class: {callerClass}";
//	//	formattedMessage += $"\nCaller Method: {callerMethod}";
//	//	formattedMessage += $"\nStackTrace: {(ex != null ? ex.StackTrace : Environment.StackTrace)}";

//	//	WriteToLog(formattedMessage);
//	//}

//	public static void LogError(string message, Exception ex = null, string originalStackTrace = null) {
//		var stackTrace = new System.Diagnostics.StackTrace();
//		string callerMethod = stackTrace.GetFrame(1)?.GetMethod()?.Name ?? "Unknown Method";
//		string callerClass = stackTrace.GetFrame(1)?.GetMethod()?.DeclaringType?.Name ?? "Unknown Class";

//		string formattedMessage = FormatLogMessage("ERROR", message);
//		formattedMessage += $"\nCaller Class: {callerClass}";
//		formattedMessage += $"\nCaller Method: {callerMethod}";
//		formattedMessage += $"\nOriginal StackTrace: {originalStackTrace ?? ex?.StackTrace ?? Environment.StackTrace}";

//		WriteToLog(formattedMessage);
//	}

//	private static string FormatLogMessage(string logLevel, string message) {
//		DateTime localTime;
//		try {
//			localTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("Singapore Standard Time"));
//		} catch {
//			localTime = DateTime.UtcNow; // Fallback to UTC if time zone fails
//		}
//		return $"[{localTime:yyyy-MM-dd HH:mm:ss}] [{logLevel}] {message}";
//	}

//	private static void WriteToLog(string message) {
//		try {
//			File.AppendAllText(logFilePath, message + Environment.NewLine);
//		} catch (Exception ex) {
//			UnityEngine.Debug.LogError($"Failed to write to log file: {ex.Message}");
//		}
//	}
//}
