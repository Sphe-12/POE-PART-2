using System;
using System.Collections.Generic;
using System.Linq;

namespace CyberChatbot
{
    // ─────────────────────────────────────────────
    //  Data class that carries a chatbot reply
    // ─────────────────────────────────────────────
    public class ChatResponse
    {
        public string Message { get; set; }
        public string Sentiment { get; set; }   // neutral / worried / curious / frustrated
        public bool   WasKeywordMatch { get; set; }
    }

    // ─────────────────────────────────────────────
    //  Core response engine  (OOP / dictionary-based)
    // ─────────────────────────────────────────────
    public class ResponseEngine
    {
        // ── Random source ──────────────────────────
        private readonly Random _rng = new Random();

        // ── User memory store ──────────────────────
        private readonly Dictionary<string, string> _memory = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        private string _lastTopic = string.Empty;

        // ── Keyword → multiple responses ──────────
        private readonly Dictionary<string, List<string>> _keywordResponses
            = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase)
        {
            {
                "password", new List<string>
                {
                    "Use strong, unique passwords for every account. Mix uppercase, lowercase, numbers and symbols.",
                    "Never reuse passwords across sites. A password manager like Bitwarden can help you keep track.",
                    "Enable two-factor authentication (2FA) wherever possible — it adds a vital second layer of security.",
                    "Avoid using personal info such as birthdays or names in your passwords; they are easy to guess."
                }
            },
            {
                "scam", new List<string>
                {
                    "Scammers often impersonate banks or government agencies. Always verify the sender before acting.",
                    "If an offer sounds too good to be true, it almost certainly is. Trust your instincts!",
                    "Never share OTPs or PINs over the phone — legitimate organisations will never ask for them.",
                    "Report suspected scams to your country's cybercrime unit to protect others."
                }
            },
            {
                "privacy", new List<string>
                {
                    "Review the privacy settings on all your social media accounts regularly.",
                    "Be careful about the personal information you share publicly online.",
                    "Use a VPN on public Wi-Fi to keep your browsing private.",
                    "Read app permission requests — many apps request access they do not actually need."
                }
            },
            {
                "phishing", new List<string>
                {
                    "Be cautious of emails asking for personal information. Scammers often disguise themselves as trusted organisations.",
                    "Check the sender's email address carefully — phishing emails often use lookalike domains.",
                    "Hover over links before clicking to see where they really lead.",
                    "When in doubt, go directly to the organisation's official website rather than clicking email links."
                }
            },
            {
                "malware", new List<string>
                {
                    "Keep your antivirus software up to date to protect against the latest threats.",
                    "Only download software from official or verified sources.",
                    "Avoid clicking on pop-up ads — they can silently install malware.",
                    "Regularly scan your device for malware, especially after visiting unfamiliar websites."
                }
            },
            {
                "firewall", new List<string>
                {
                    "Always keep your operating system's built-in firewall enabled.",
                    "A firewall monitors incoming and outgoing traffic and blocks suspicious connections.",
                    "Consider a hardware firewall for extra protection on your home network."
                }
            },
            {
                "wifi", new List<string>
                {
                    "Avoid accessing sensitive accounts (banking, email) on public Wi-Fi without a VPN.",
                    "Use WPA3 encryption on your home router if your hardware supports it.",
                    "Change your router's default admin password to prevent unauthorised access."
                }
            },
            {
                "ransomware", new List<string>
                {
                    "Back up your important files regularly — ransomware can encrypt everything and demand payment.",
                    "Never pay a ransom; it does not guarantee data recovery and encourages more attacks.",
                    "Keep all software patched; ransomware often exploits known vulnerabilities."
                }
            },
            {
                "social engineering", new List<string>
                {
                    "Social engineers manipulate people psychologically rather than hacking technology.",
                    "Always verify the identity of anyone requesting sensitive information, even if they seem legitimate.",
                    "Be wary of urgent requests — urgency is a classic social-engineering tactic."
                }
            }
        };

        // ── Sentiment keywords ─────────────────────
        private readonly Dictionary<string, List<string>> _sentimentWords
            = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase)
        {
            { "worried",    new List<string> { "worried", "scared", "afraid", "nervous", "anxious", "fear", "terrified", "unsafe" } },
            { "curious",    new List<string> { "curious", "wondering", "interested", "want to know", "tell me", "how does", "what is", "explain" } },
            { "frustrated", new List<string> { "frustrated", "angry", "annoyed", "fed up", "sick of", "hate", "ridiculous", "useless" } }
        };

        // ── Sentiment response prefixes ────────────
        private readonly Dictionary<string, string> _sentimentPrefixes
            = new Dictionary<string, string>
        {
            { "worried",    "It's completely understandable to feel that way. Let me share some tips to help you stay safe.\n\n" },
            { "curious",    "Great question! I love your curiosity about cybersecurity. Here's what you need to know:\n\n" },
            { "frustrated", "I hear you — cybersecurity can feel overwhelming. Let me break this down simply for you.\n\n" },
            { "neutral",    "" }
        };

        // ── Follow-up triggers ─────────────────────
        private readonly List<string> _moreRequestTriggers = new List<string>
        {
            "tell me more", "more", "another tip", "explain more", "give me another",
            "more info", "continue", "elaborate", "go on", "keep going"
        };

        // ─────────────────────────────────────────
        //  Public API
        // ─────────────────────────────────────────

        /// <summary>Process a user message and return a ChatResponse.</summary>
        public ChatResponse ProcessInput(string userInput)
        {
            if (string.IsNullOrWhiteSpace(userInput))
                return Build("Please type something so I can help you!", "neutral", false);

            string input     = userInput.Trim();
            string sentiment = DetectSentiment(input);

            // 1. Check for greeting
            if (IsGreeting(input))
            {
                string name = _memory.ContainsKey("name") ? $", {_memory["name"]}" : "";
                return Build($"Hello{name}! 👋 I'm your Cybersecurity Awareness Assistant. " +
                             "Ask me about passwords, phishing, scams, privacy, malware, or any cybersecurity topic!", sentiment, false);
            }

            // 2. Memory capture: "my name is ..."
            if (TryCaptureName(input, out string capturedName))
            {
                _memory["name"] = capturedName;
                return Build($"Great to meet you, {capturedName}! 😊 I'll remember your name. " +
                             "Feel free to ask me anything about staying safe online.", sentiment, false);
            }

            // 3. Memory capture: "I'm interested in ..."
            if (TryCaptureInterest(input, out string capturedInterest))
            {
                _memory["interest"] = capturedInterest;
                return Build($"Great! I'll remember that you're interested in {capturedInterest}. " +
                             $"It's a crucial part of staying safe online. Ask me anything about it!", sentiment, false);
            }

            // 4. "What do you remember / what's my name" queries
            if (IsMemoryQuery(input))
                return HandleMemoryQuery(sentiment);

            // 5. Follow-up: "tell me more" / "another tip"
            if (_moreRequestTriggers.Any(t => input.IndexOf(t, StringComparison.OrdinalIgnoreCase) >= 0))
            {
                if (!string.IsNullOrEmpty(_lastTopic) && _keywordResponses.ContainsKey(_lastTopic))
                {
                    string tip = PickRandom(_keywordResponses[_lastTopic]);
                    string prefix = _sentimentPrefixes[sentiment];
                    return Build(prefix + $"Here's another tip on {_lastTopic}:\n{tip}", sentiment, true);
                }
                return Build("Sure! What topic would you like more information on? Try asking about passwords, phishing, scams, or privacy.", sentiment, false);
            }

            // 6. Keyword matching
            foreach (var kv in _keywordResponses)
            {
                if (input.IndexOf(kv.Key, StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    _lastTopic = kv.Key;
                    string tip    = PickRandom(kv.Value);
                    string prefix = _sentimentPrefixes[sentiment];

                    // Personalise if we know their interest
                    string personalisation = "";
                    if (_memory.ContainsKey("interest") &&
                        kv.Key.IndexOf(_memory["interest"], StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        personalisation = $" As someone interested in {_memory["interest"]}, this is especially relevant for you.";
                    }

                    return Build(prefix + tip + personalisation, sentiment, true);
                }
            }

            // 7. Default / unknown input
            return Build("I'm not sure I understand. Can you try rephrasing? 🤔\n" +
                         "You can ask me about: passwords, phishing, scams, privacy, malware, ransomware, firewall, or Wi-Fi safety.",
                         sentiment, false);
        }

        // ─────────────────────────────────────────
        //  Helper methods
        // ─────────────────────────────────────────

        private string DetectSentiment(string input)
        {
            foreach (var kv in _sentimentWords)
                foreach (var word in kv.Value)
                    if (input.IndexOf(word, StringComparison.OrdinalIgnoreCase) >= 0)
                        return kv.Key;
            return "neutral";
        }

        private bool IsGreeting(string input)
        {
            string[] greetings = { "hello", "hi", "hey", "good morning", "good afternoon", "good evening", "howdy" };
            return greetings.Any(g => input.IndexOf(g, StringComparison.OrdinalIgnoreCase) >= 0);
        }

        private bool TryCaptureName(string input, out string name)
        {
            name = string.Empty;
            string[] patterns = { "my name is ", "i am ", "i'm ", "call me " };
            foreach (var p in patterns)
            {
                int idx = input.IndexOf(p, StringComparison.OrdinalIgnoreCase);
                if (idx >= 0)
                {
                    string candidate = input.Substring(idx + p.Length).Trim().Split(' ')[0].Trim('.', ',', '!', '?');
                    if (candidate.Length > 1)
                    {
                        name = char.ToUpper(candidate[0]) + candidate.Substring(1);
                        return true;
                    }
                }
            }
            return false;
        }

        private bool TryCaptureInterest(string input, out string interest)
        {
            interest = string.Empty;
            string[] patterns = { "i'm interested in ", "i am interested in ", "interested in ", "i care about " };
            foreach (var p in patterns)
            {
                int idx = input.IndexOf(p, StringComparison.OrdinalIgnoreCase);
                if (idx >= 0)
                {
                    interest = input.Substring(idx + p.Length).Trim().TrimEnd('.', '!', '?');
                    return interest.Length > 0;
                }
            }
            return false;
        }

        private bool IsMemoryQuery(string input)
        {
            string[] queries = { "what's my name", "what is my name", "do you remember", "what do you know about me", "my interest" };
            return queries.Any(q => input.IndexOf(q, StringComparison.OrdinalIgnoreCase) >= 0);
        }

        private ChatResponse HandleMemoryQuery(string sentiment)
        {
            if (_memory.Count == 0)
                return Build("I don't have any information about you yet. Tell me your name or what cybersecurity topics interest you!", sentiment, false);

            var sb = new System.Text.StringBuilder("Here's what I remember about you:\n");
            if (_memory.ContainsKey("name"))     sb.AppendLine($"  • Your name: {_memory["name"]}");
            if (_memory.ContainsKey("interest")) sb.AppendLine($"  • Your interest: {_memory["interest"]}");
            return Build(sb.ToString().TrimEnd(), sentiment, false);
        }

        private string PickRandom(List<string> items) => items[_rng.Next(items.Count)];

        private static ChatResponse Build(string msg, string sentiment, bool keyword) =>
            new ChatResponse { Message = msg, Sentiment = sentiment, WasKeywordMatch = keyword };
    }
}
