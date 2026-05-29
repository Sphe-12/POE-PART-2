# POE — Part 2

## Description
A **WinForms GUI** cybersecurity awareness chatbot built in C# (.NET 6). This is Part 2 of the project, expanding the console chatbot with a full graphical interface, dynamic responses, sentiment detection, and memory features.

---

## Features

| Feature | Details |
|---|---|
| **GUI (WinForms)** | Dark-theme chat interface with a header (ASCII art logo), scrollable chat area, quick-topic buttons, and an input footer |
| **Keyword Recognition** | Recognises: `password`, `scam`, `privacy`, `phishing`, `malware`, `firewall`, `wifi`, `ransomware`, `social engineering` |
| **Random Responses** | Multiple predefined responses per topic; randomly selected each time via `List<string>` + `Random` |
| **Conversation Flow** | Supports follow-up phrases like *"tell me more"*, *"another tip"*, *"explain more"* to continue the current topic |
| **Memory & Recall** | Stores the user's **name** and **interest** and references them in later responses |
| **Sentiment Detection** | Detects `worried`, `curious`, `frustrated` sentiments and adjusts the chatbot prefix/tone accordingly |
| **Error Handling** | Default response for unknown inputs; no crashes on empty or unexpected input |
| **OOP Design** | `ResponseEngine` class handles all logic; `MainForm` handles the UI; `ChatResponse` is a data-transfer object |

---

## How to Run

### Requirements
- Windows OS
- [.NET 6 SDK](https://dotnet.microsoft.com/download/dotnet/6.0) or Visual Studio 2022

### Steps
```bash
git clone <your-repo-url>
cd CyberChatbot
dotnet run
```
Or open `CyberChatbot.csproj` in Visual Studio and press **F5**.

---

## Project Structure

```
CyberChatbot/
│
├── Program.cs          # Entry point (STAThread)
├── MainForm.cs         # WinForms GUI — all controls built programmatically
├── ResponseEngine.cs   # Core chatbot logic — keywords, sentiment, memory, random responses
├── CyberChatbot.csproj # .NET 6 WinForms project file
└── README.md           # This file
```

---

## Example Interactions

```
User:    "Tell me about password safety."
Chatbot: "Use strong, unique passwords for every account. Mix uppercase, lowercase, numbers and symbols."

User:    "I'm worried about online scams."
Chatbot: "It's completely understandable to feel that way. Let me share some tips...
          Scammers often impersonate banks or government agencies."

User:    "My name is Alex"
Chatbot: "Great to meet you, Alex! 😊 I'll remember your name."

User:    "Tell me more"
Chatbot: "Here's another tip on scams: Never share OTPs or PINs over the phone."
```

---

## GitHub Commits (minimum 6)
1. `Initial project setup — WinForms skeleton`
2. `Add ResponseEngine with keyword dictionary`
3. `Add random response selection with List<string>`
4. `Implement memory recall (name + interest)`
5. `Add sentiment detection and adaptive responses`
6. `Add conversation flow (follow-up handling)`
7. `Polish GUI: dark theme, quick-topic buttons, status bar`
8. `Final cleanup and README`

---

## Releases
- **v1.0** — Core keyword recognition + GUI layout
- **v2.0** — Full feature set: memory, sentiment, conversation flow, error handling
