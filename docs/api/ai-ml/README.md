# 🤖 AI/ML APIs

**APIs:** 3 (OpenAI GPT-4, Google Vision, TensorFlow.js)  
**Estado:** En Implementación (Fase 1-3)  
**Prioridad:** 🔴 CRÍTICA

---

## 📖 Resumen

Inteligencia Artificial para recomendaciones, análisis de imágenes y chatbot.

### Casos de Uso

✅ Chatbot de soporte  
✅ Análisis de fotos de vehículos  
✅ Recomendaciones personalizadas  
✅ Detección de fraude  
✅ Pricing inteligente

---

## 🔗 APIs

| API               | Costo                | Caso Uso       |
| ----------------- | -------------------- | -------------- |
| **OpenAI GPT-4**  | $0.03/1K tokens      | Chatbot        |
| **Google Vision** | $0.60/1K requests    | Análisis fotos |
| **TensorFlow.js** | Gratis (self-hosted) | ML en frontend |

---

## 💻 OpenAI Implementation

```csharp
public interface IAiService
{
    Task<string> GenerateResponseAsync(string userMessage);
    Task<List<string>> GenerateRecommendationsAsync(Vehicle vehicle);
}

public class OpenAiService : IAiService
{
    private readonly OpenAIClient _openAiClient;

    public async Task<string> GenerateResponseAsync(string userMessage)
    {
        var messages = new[]
        {
            new ChatCompletionRequestSystemMessage("Eres un agente de soporte de OKLA"),
            new ChatCompletionRequestUserMessage(userMessage)
        };

        var response = await _openAiClient.CreateChatCompletionAsync(
            messages: messages.ToList(),
            model: "gpt-4"
        );

        return response.Choices[0].Message.Content;
    }

    public async Task<List<string>> GenerateRecommendationsAsync(Vehicle vehicle)
    {
        var prompt = $"Recomienda 5 vehículos similares a: {vehicle.Make} {vehicle.Model}";
        var response = await _openAiClient.CreateChatCompletionAsync(
            messages: new[] {
                new ChatCompletionRequestUserMessage(prompt)
            }.ToList(),
            model: "gpt-4"
        );

        return ParseRecommendations(response.Choices[0].Message.Content);
    }
}
```

### Frontend Chatbot

```typescript
// useChatbot.ts
export const useChatbot = () => {
  const [messages, setMessages] = useState<Message[]>([]);
  const sendMessage = useMutation({
    mutationFn: async (content: string) => {
      return api.post("/api/ai/chat", { message: content });
    },
  });

  return { messages, sendMessage };
};

// ChatWidget.tsx
export const ChatWidget = () => {
  const { messages, sendMessage } = useChatbot();
  const [input, setInput] = useState("");

  return (
    <div className="chat-widget">
      <div className="messages">
        {messages.map((m) => (
          <div key={m.id} className={m.role}>
            {m.content}
          </div>
        ))}
      </div>
      <input
        value={input}
        onChange={(e) => setInput(e.target.value)}
        onKeyPress={(e) => {
          if (e.key === "Enter") sendMessage.mutate(input);
        }}
      />
    </div>
  );
};
```

---

**Versión:** 1.0 | **Actualizado:** Enero 15, 2026
