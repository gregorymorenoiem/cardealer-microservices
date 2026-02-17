'use client';

/**
 * Página de Prueba del Chatbot OKLA
 * ==================================
 * Página provisional para probar el modelo LLM fine-tuned con diferentes dealers.
 * Permite seleccionar un dealer y chatear directamente con el bot,
 * viendo tanto la respuesta formateada como el JSON raw.
 *
 * Ruta: /test-chatbot
 * Uso: Solo desarrollo — no debe ir a producción.
 */

import { useState, useRef, useEffect, useCallback } from 'react';
import { Send, Bot, User, RefreshCw, Code, Eye, Building2, Trash2 } from 'lucide-react';

// ============================================================
// Types
// ============================================================

interface DealerOption {
  id: string;
  name: string;
  botName: string;
  description: string;
  style: string;
  configId: string;
}

interface ChatMsg {
  id: string;
  role: 'user' | 'assistant' | 'system';
  content: string;
  rawJson?: string;
  parsedResponse?: ParsedLlmResponse | null;
  timestamp: Date;
  responseTimeMs?: number;
  tokensUsed?: number;
  intentName?: string;
  confidenceScore?: number;
  isFallback?: boolean;
  vehicleCards?: VehicleCardData[];
  quickReplies?: string[];
}

interface ParsedLlmResponse {
  response?: string;
  intent?: string;
  confidence?: number;
  isFallback?: boolean;
  parameters?: Record<string, string>;
  leadSignals?: {
    mentionedBudget?: boolean;
    requestedTestDrive?: boolean;
    askedFinancing?: boolean;
    providedContactInfo?: boolean;
  };
  suggestedAction?: string | null;
  quickReplies?: string[];
}

interface VehicleCardData {
  vehicleId: string;
  title: string;
  subtitle: string;
  imageUrl: string;
  price: number;
}

interface LlmHealthResponse {
  status: string;
  model_loaded: boolean;
  model_path: string;
  uptime_seconds: number;
  total_requests: number;
  avg_response_time_ms: number;
}

// ============================================================
// Constants
// ============================================================

const DEALERS: DealerOption[] = [
  {
    id: '11111111-1111-1111-1111-111111111111',
    configId: 'a1b2c3d4-e5f6-7890-abcd-ef1234567890',
    name: 'Auto Dominicana Premium',
    botName: 'Ana',
    description: 'Dealer premium en Santo Domingo — tono profesional y formal',
    style: 'bg-blue-600',
  },
  {
    id: '22222222-2222-2222-2222-222222222222',
    configId: 'b2c3d4e5-f6a7-8901-bcde-f12345678901',
    name: 'MotorMax RD',
    botName: 'Carlos',
    description: 'Dealer popular en Santiago — tono informal y coloquial',
    style: 'bg-orange-600',
  },
];

// ============================================================
// LLM Server URL (direct, bypasses Gateway for testing)
// ============================================================
const LLM_SERVER_URL = process.env.NEXT_PUBLIC_LLM_SERVER_URL || 'http://localhost:8000';
const CHATBOT_API_URL = process.env.NEXT_PUBLIC_API_URL || 'http://localhost:5060';

// ============================================================
// System Prompts per Dealer
// ============================================================

function buildSystemPrompt(dealer: DealerOption, vehicles: VehicleCardData[]): string {
  const vehicleList = vehicles
    .map((v, i) => `${i + 1}. ${v.title} | RD$${v.price.toLocaleString('es-DO')} | ${v.subtitle}`)
    .join('\n');

  if (dealer.id === '11111111-1111-1111-1111-111111111111') {
    return `Eres Ana, el asistente virtual de Auto Dominicana Premium, un concesionario de vehículos premium en República Dominicana que opera dentro de la plataforma OKLA (okla.com.do).

IDENTIDAD Y PERSONALIDAD:
- Tu nombre es Ana.
- Representas a Auto Dominicana Premium.
- Tu tono es profesional pero cercano, cálido y servicial.
- Hablas en español dominicano neutro — profesional con calidez caribeña.
- Entiendes modismos: "yipeta" (SUV), "guagua" (vehículo), "carro" (auto), "pela'o" (barato), "chivo" (oferta), "tato" (ok).
- NUNCA inventes información. Sé conciso (2-4 oraciones). Usa emojis moderadamente (1-2).

INFORMACIÓN DEL DEALER:
- Nombre: Auto Dominicana Premium
- Teléfono: 809-555-0101
- Dirección: Av. 27 de Febrero #456, Santo Domingo
- Horarios: Lun-Vie 8AM-6PM, Sáb 9AM-2PM
- Financiamiento: BHD León, Banreservas
- Trade-in: Sí | Taller: Sí

INVENTARIO ACTUAL:
${vehicleList}

CUMPLIMIENTO LEGAL:
- Ley 358-05: Precios en RD$. NUNCA "precio final". Agregar "*Precio de referencia sujeto a confirmación."
- Ley 172-13: NUNCA solicitar cédula o tarjeta por chat.
- DGII: Precios NO incluyen traspaso ni impuestos.

FORMATO: Responde SIEMPRE en JSON con: response, intent, confidence, isFallback, parameters, leadSignals, suggestedAction, quickReplies.`;
  }

  return `Eres Carlos, el asistente virtual de MotorMax RD, un dealer de carros nuevos y usados en Santiago, República Dominicana. Operas en la plataforma OKLA (okla.com.do).

IDENTIDAD Y PERSONALIDAD:
- Tu nombre es Carlos.
- Representas a MotorMax RD.
- Tu tono es informal, amigable y entusiasta — como un pana que sabe de carros.
- Hablas en español dominicano coloquial — usas "klk", "tato", "dimelo", "tranqui".
- Argot: "yipeta" (SUV), "pela'o" (barato), "chivo" (oferta), "un palo" (1 millón), "vaina" (cosa).
- Directo y al grano. Usa emojis frecuentemente 🔥🚗💰 (2-3 por mensaje).
- NUNCA inventes precios. Si no sabes, ofrece conectar con vendedor.

INFORMACIÓN DEL DEALER:
- Nombre: MotorMax RD
- Teléfono: 809-555-0202
- Dirección: Av. Máximo Gómez #789, Santiago
- Horarios: Lun-Sáb 9AM-7PM
- Financiamiento: Banco Popular, BHD, Asociación Popular
- Trade-in: Sí (recibimos tu carro viejo)

INVENTARIO ACTUAL:
${vehicleList}

CUMPLIMIENTO LEGAL:
- Ley 358-05: Precios en RD$. No "precio final". Disclaimer de referencia.
- Ley 172-13: No pedir cédula ni tarjetas por chat.
- DGII: Precios no incluyen traspaso ni impuestos.

FORMATO: Responde SIEMPRE en JSON con: response, intent, confidence, isFallback, parameters, leadSignals, suggestedAction, quickReplies.`;
}

// ============================================================
// Fake vehicle data (matches seed-data.sql)
// ============================================================

const DEALER_VEHICLES: Record<string, VehicleCardData[]> = {
  '11111111-1111-1111-1111-111111111111': [
    {
      vehicleId: '1',
      title: 'Toyota RAV4 2024 XLE',
      subtitle: 'SUV | Gasolina | Automática | 5,000km | Blanco',
      imageUrl: '',
      price: 2850000,
    },
    {
      vehicleId: '2',
      title: 'Hyundai Tucson 2024 SEL',
      subtitle: 'SUV | Gasolina | Automática | 3,000km | Negro',
      imageUrl: '',
      price: 2450000,
    },
    {
      vehicleId: '3',
      title: 'Honda CR-V 2023 EX-L',
      subtitle: 'SUV | Gasolina | CVT | 8,000km | Gris',
      imageUrl: '',
      price: 3100000,
    },
    {
      vehicleId: '4',
      title: 'Kia Sportage 2024 LX',
      subtitle: 'SUV | Gasolina | Automática | 4,000km | Rojo',
      imageUrl: '',
      price: 1950000,
    },
    {
      vehicleId: '5',
      title: 'Toyota Corolla 2024 SE',
      subtitle: 'Sedán | Gasolina | CVT | 2,000km | Azul',
      imageUrl: '',
      price: 1650000,
    },
    {
      vehicleId: '6',
      title: 'Honda Civic 2024 Sport',
      subtitle: 'Sedán | Gasolina | CVT | 1,500km | Negro',
      imageUrl: '',
      price: 1800000,
    },
    {
      vehicleId: '7',
      title: 'Mitsubishi L200 2024 GLS',
      subtitle: 'Pickup | Diesel | Manual | 3,500km | Plateado',
      imageUrl: '',
      price: 2600000,
    },
    {
      vehicleId: '8',
      title: 'Hyundai Sonata 2023 Limited',
      subtitle: 'Sedán | Gasolina | Automática | 12,000km | Blanco ¡OFERTA!',
      imageUrl: '',
      price: 2200000,
    },
  ],
  '22222222-2222-2222-2222-222222222222': [
    {
      vehicleId: '9',
      title: 'Toyota Hilux 2022 SR5',
      subtitle: 'Pickup | Diesel | Automática | 25,000km | Blanco ¡OFERTA!',
      imageUrl: '',
      price: 2100000,
    },
    {
      vehicleId: '10',
      title: 'Hyundai Accent 2023 GL',
      subtitle: 'Sedán | Gasolina | Automática | 15,000km | Gris',
      imageUrl: '',
      price: 1050000,
    },
    {
      vehicleId: '11',
      title: 'Kia K5 2023 GT-Line',
      subtitle: 'Sedán | Gasolina | Automática | 18,000km | Rojo ¡OFERTA!',
      imageUrl: '',
      price: 1750000,
    },
    {
      vehicleId: '12',
      title: 'Suzuki Vitara 2022 GLX',
      subtitle: 'SUV | Gasolina | Automática | 20,000km | Verde',
      imageUrl: '',
      price: 1450000,
    },
    {
      vehicleId: '13',
      title: 'Toyota Yaris 2021 XLE',
      subtitle: 'Sedán | Gasolina | CVT | 35,000km | Blanco',
      imageUrl: '',
      price: 850000,
    },
    {
      vehicleId: '14',
      title: 'Nissan Kicks 2023 SR',
      subtitle: 'SUV | Gasolina | CVT | 12,000km | Naranja',
      imageUrl: '',
      price: 1350000,
    },
    {
      vehicleId: '15',
      title: 'Honda HR-V 2022 EX',
      subtitle: 'SUV | Gasolina | CVT | 22,000km | Azul ¡OFERTA!',
      imageUrl: '',
      price: 1550000,
    },
  ],
};

// ============================================================
// Component
// ============================================================

export default function TestChatbotPage() {
  const [selectedDealer, setSelectedDealer] = useState<DealerOption>(DEALERS[0]);
  const [messages, setMessages] = useState<ChatMsg[]>([]);
  const [input, setInput] = useState('');
  const [isLoading, setIsLoading] = useState(false);
  const [showRawJson, setShowRawJson] = useState(false);
  const [llmHealth, setLlmHealth] = useState<LlmHealthResponse | null>(null);
  const [llmError, setLlmError] = useState<string | null>(null);
  const [useDirectLlm, setUseDirectLlm] = useState(true);
  const messagesEndRef = useRef<HTMLDivElement>(null);
  const inputRef = useRef<HTMLTextAreaElement>(null);

  // Auto-scroll to bottom
  useEffect(() => {
    messagesEndRef.current?.scrollIntoView({ behavior: 'smooth' });
  }, [messages]);

  // Check LLM health on mount
  useEffect(() => {
    checkLlmHealth();
  }, []);

  const checkLlmHealth = async () => {
    try {
      setLlmError(null);
      const res = await fetch(`${LLM_SERVER_URL}/health`);
      if (res.ok) {
        const data = await res.json();
        setLlmHealth(data);
      } else {
        setLlmError(`LLM Server respondió con ${res.status}`);
      }
    } catch {
      setLlmError('No se pudo conectar al LLM Server. ¿Está corriendo docker-compose?');
    }
  };

  const clearChat = () => {
    setMessages([]);
  };

  const switchDealer = (dealer: DealerOption) => {
    setSelectedDealer(dealer);
    setMessages([]);
  };

  // ── Send message directly to LLM Server ──────────────
  const sendDirectToLlm = useCallback(
    async (userMessage: string) => {
      const vehicles = DEALER_VEHICLES[selectedDealer.id] || [];
      const systemPrompt = buildSystemPrompt(selectedDealer, vehicles);

      // Build conversation history
      const conversationMessages = [
        { role: 'system', content: systemPrompt },
        ...messages
          .filter(m => m.role !== 'system')
          .slice(-6) // Last 6 messages for context
          .map(m => ({ role: m.role, content: m.content })),
        { role: 'user', content: userMessage },
      ];

      const res = await fetch(`${LLM_SERVER_URL}/v1/chat/completions`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({
          model: 'okla-llama3-8b',
          messages: conversationMessages,
          temperature: 0.7,
          top_p: 0.9,
          max_tokens: 512,
          repetition_penalty: 1.1,
        }),
      });

      if (!res.ok) {
        throw new Error(`LLM Server error: ${res.status} ${res.statusText}`);
      }

      return await res.json();
    },
    [selectedDealer, messages]
  );

  // ── Send message via ChatbotService (.NET API) ────────
  const sendViaChatbotApi = useCallback(async (userMessage: string, sessionToken: string) => {
    const res = await fetch(`${CHATBOT_API_URL}/api/chat/message`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({
        sessionToken,
        message: userMessage,
        type: 'UserText',
      }),
    });

    if (!res.ok) {
      throw new Error(`ChatbotService error: ${res.status} ${res.statusText}`);
    }

    return await res.json();
  }, []);

  // ── Main send handler ─────────────────────────────────
  const handleSend = async () => {
    const trimmed = input.trim();
    if (!trimmed || isLoading) return;

    // Add user message
    const userMsg: ChatMsg = {
      id: `user-${Date.now()}`,
      role: 'user',
      content: trimmed,
      timestamp: new Date(),
    };
    setMessages(prev => [...prev, userMsg]);
    setInput('');
    setIsLoading(true);

    try {
      const startTime = Date.now();

      if (useDirectLlm) {
        // Direct to LLM Server
        const result = await sendDirectToLlm(trimmed);
        const elapsed = Date.now() - startTime;
        const rawContent = result?.choices?.[0]?.message?.content || '';

        // Try to parse JSON from response
        let parsed: ParsedLlmResponse | null = null;
        let displayText = rawContent;
        try {
          const jsonStart = rawContent.indexOf('{');
          const jsonEnd = rawContent.lastIndexOf('}');
          if (jsonStart >= 0 && jsonEnd > jsonStart) {
            parsed = JSON.parse(rawContent.substring(jsonStart, jsonEnd + 1));
            displayText = parsed?.response || rawContent;
          }
        } catch {
          // Not valid JSON — use raw text
        }

        const botMsg: ChatMsg = {
          id: `bot-${Date.now()}`,
          role: 'assistant',
          content: displayText,
          rawJson: rawContent,
          parsedResponse: parsed,
          timestamp: new Date(),
          responseTimeMs: elapsed,
          tokensUsed: result?.usage?.total_tokens,
          intentName: parsed?.intent,
          confidenceScore: parsed?.confidence,
          isFallback: parsed?.isFallback,
          quickReplies: parsed?.quickReplies,
        };
        setMessages(prev => [...prev, botMsg]);
      } else {
        // Via ChatbotService API
        const result = await sendViaChatbotApi(trimmed, 'test-session');
        const elapsed = Date.now() - startTime;

        const botMsg: ChatMsg = {
          id: result.messageId || `bot-${Date.now()}`,
          role: 'assistant',
          content: result.response,
          rawJson: JSON.stringify(result, null, 2),
          timestamp: new Date(),
          responseTimeMs: result.responseTimeMs || elapsed,
          intentName: result.intentName,
          confidenceScore: result.confidenceScore,
          isFallback: result.isFallback,
        };
        setMessages(prev => [...prev, botMsg]);
      }
    } catch (err: unknown) {
      const error = err as Error;
      const errorMsg: ChatMsg = {
        id: `error-${Date.now()}`,
        role: 'assistant',
        content: `❌ Error: ${error.message}`,
        timestamp: new Date(),
        isFallback: true,
      };
      setMessages(prev => [...prev, errorMsg]);
    } finally {
      setIsLoading(false);
      inputRef.current?.focus();
    }
  };

  const handleKeyDown = (e: React.KeyboardEvent) => {
    if (e.key === 'Enter' && !e.shiftKey) {
      e.preventDefault();
      handleSend();
    }
  };

  // ── Quick test messages ───────────────────────────────
  const quickTests = [
    'Klk! Busco una yipeta buena',
    'Tienen algo por debajo de 2 millones?',
    'Quiero probar la RAV4, puedo ir mañana?',
    'Mi cédula es 001-1234567-8',
    'Esa RAV4 está muy cara',
    'Cuál es mejor, la RAV4 o la Tucson?',
    'Necesito hablar con una persona real',
    'Tienen financiamiento disponible?',
  ];

  return (
    <div className="min-h-screen bg-gray-50">
      {/* Header */}
      <header className="sticky top-0 z-10 border-b bg-white">
        <div className="mx-auto max-w-7xl px-4 py-3">
          <div className="flex items-center justify-between">
            <div className="flex items-center gap-3">
              <Bot className="h-8 w-8 text-green-600" />
              <div>
                <h1 className="text-xl font-bold text-gray-900">OKLA Chatbot — Test Page</h1>
                <p className="text-sm text-gray-500">
                  Prueba del modelo LLM fine-tuned (provisional)
                </p>
              </div>
            </div>

            <div className="flex items-center gap-3">
              {/* LLM Health */}
              <div className="flex items-center gap-2">
                <div
                  className={`h-3 w-3 rounded-full ${
                    llmHealth?.model_loaded
                      ? 'bg-green-500'
                      : llmError
                        ? 'bg-red-500'
                        : 'bg-yellow-500'
                  }`}
                />
                <span className="text-xs text-gray-600">
                  {llmHealth?.model_loaded
                    ? `LLM OK (${llmHealth.total_requests} reqs)`
                    : llmError
                      ? 'LLM Offline'
                      : 'Checking...'}
                </span>
                <button
                  onClick={checkLlmHealth}
                  className="rounded p-1 hover:bg-gray-100"
                  title="Refresh health"
                >
                  <RefreshCw className="h-3 w-3" />
                </button>
              </div>

              {/* Toggle Raw JSON */}
              <button
                onClick={() => setShowRawJson(!showRawJson)}
                className={`flex items-center gap-1 rounded-lg px-3 py-1.5 text-sm font-medium transition-colors ${
                  showRawJson
                    ? 'bg-gray-800 text-white'
                    : 'bg-gray-100 text-gray-700 hover:bg-gray-200'
                }`}
              >
                {showRawJson ? <Code className="h-4 w-4" /> : <Eye className="h-4 w-4" />}
                {showRawJson ? 'JSON' : 'Vista'}
              </button>
            </div>
          </div>
        </div>
      </header>

      <div className="mx-auto flex max-w-7xl gap-6 px-4 py-6">
        {/* ── Sidebar: Dealer Selector + Info ──────────── */}
        <aside className="w-80 flex-shrink-0 space-y-4">
          {/* Dealer Selection */}
          <div className="rounded-xl border bg-white p-4 shadow-sm">
            <h2 className="mb-3 flex items-center gap-2 text-sm font-semibold text-gray-700">
              <Building2 className="h-4 w-4" /> Seleccionar Dealer
            </h2>
            <div className="space-y-2">
              {DEALERS.map(dealer => (
                <button
                  key={dealer.id}
                  onClick={() => switchDealer(dealer)}
                  className={`w-full rounded-lg border-2 p-3 text-left transition-all ${
                    selectedDealer.id === dealer.id
                      ? 'border-green-500 bg-green-50'
                      : 'border-gray-200 bg-white hover:border-gray-300'
                  }`}
                >
                  <div className="flex items-center gap-2">
                    <div
                      className={`h-8 w-8 rounded-full ${dealer.style} flex items-center justify-center`}
                    >
                      <span className="text-xs font-bold text-white">{dealer.botName[0]}</span>
                    </div>
                    <div>
                      <div className="text-sm font-medium text-gray-900">{dealer.name}</div>
                      <div className="text-xs text-gray-500">Bot: {dealer.botName}</div>
                    </div>
                  </div>
                  <p className="mt-1 text-xs text-gray-500">{dealer.description}</p>
                </button>
              ))}
            </div>
          </div>

          {/* Connection Mode */}
          <div className="rounded-xl border bg-white p-4 shadow-sm">
            <h2 className="mb-3 text-sm font-semibold text-gray-700">Modo de Conexión</h2>
            <div className="space-y-2">
              <label className="flex cursor-pointer items-center gap-2">
                <input
                  type="radio"
                  checked={useDirectLlm}
                  onChange={() => setUseDirectLlm(true)}
                  className="text-green-600"
                />
                <div>
                  <div className="text-sm font-medium">Directo al LLM</div>
                  <div className="text-xs text-gray-500">localhost:8000 (más rápido)</div>
                </div>
              </label>
              <label className="flex cursor-pointer items-center gap-2">
                <input
                  type="radio"
                  checked={!useDirectLlm}
                  onChange={() => setUseDirectLlm(false)}
                  className="text-green-600"
                />
                <div>
                  <div className="text-sm font-medium">Via ChatbotService</div>
                  <div className="text-xs text-gray-500">localhost:5060 (.NET API)</div>
                </div>
              </label>
            </div>
          </div>

          {/* Quick Tests */}
          <div className="rounded-xl border bg-white p-4 shadow-sm">
            <h2 className="mb-3 text-sm font-semibold text-gray-700">Pruebas Rápidas</h2>
            <div className="space-y-1.5">
              {quickTests.map((test, i) => (
                <button
                  key={i}
                  onClick={() => {
                    setInput(test);
                    inputRef.current?.focus();
                  }}
                  disabled={isLoading}
                  className="w-full rounded-lg bg-gray-50 px-3 py-2 text-left text-xs text-gray-700 transition-colors hover:bg-gray-100 disabled:opacity-50"
                >
                  {test}
                </button>
              ))}
            </div>
          </div>

          {/* Inventory Summary */}
          <div className="rounded-xl border bg-white p-4 shadow-sm">
            <h2 className="mb-3 text-sm font-semibold text-gray-700">
              Inventario de {selectedDealer.name}
            </h2>
            <div className="max-h-60 space-y-1.5 overflow-y-auto">
              {(DEALER_VEHICLES[selectedDealer.id] || []).map(v => (
                <div key={v.vehicleId} className="rounded bg-gray-50 p-2 text-xs">
                  <div className="font-medium text-gray-800">{v.title}</div>
                  <div className="font-semibold text-green-600">
                    RD$ {v.price.toLocaleString('es-DO')}
                  </div>
                </div>
              ))}
            </div>
          </div>

          {/* LLM Stats */}
          {llmHealth && (
            <div className="rounded-xl border bg-white p-4 shadow-sm">
              <h2 className="mb-3 text-sm font-semibold text-gray-700">LLM Server Stats</h2>
              <div className="space-y-1 text-xs text-gray-600">
                <div className="flex justify-between">
                  <span>Estado:</span>
                  <span className={llmHealth.model_loaded ? 'text-green-600' : 'text-red-600'}>
                    {llmHealth.status}
                  </span>
                </div>
                <div className="flex justify-between">
                  <span>Modelo cargado:</span>
                  <span>{llmHealth.model_loaded ? '✅' : '❌'}</span>
                </div>
                <div className="flex justify-between">
                  <span>Requests totales:</span>
                  <span>{llmHealth.total_requests}</span>
                </div>
                <div className="flex justify-between">
                  <span>Tiempo promedio:</span>
                  <span>{llmHealth.avg_response_time_ms.toFixed(0)}ms</span>
                </div>
                <div className="flex justify-between">
                  <span>Uptime:</span>
                  <span>{(llmHealth.uptime_seconds / 60).toFixed(0)} min</span>
                </div>
              </div>
            </div>
          )}
        </aside>

        {/* ── Main Chat Area ──────────────────────────── */}
        <main
          className="flex flex-1 flex-col overflow-hidden rounded-xl border bg-white shadow-sm"
          style={{ height: 'calc(100vh - 130px)' }}
        >
          {/* Chat header */}
          <div
            className={`${selectedDealer.style} flex items-center justify-between px-5 py-3 text-white`}
          >
            <div className="flex items-center gap-3">
              <div className="flex h-10 w-10 items-center justify-center rounded-full bg-white/20">
                <Bot className="h-6 w-6" />
              </div>
              <div>
                <div className="font-semibold">
                  {selectedDealer.botName} — {selectedDealer.name}
                </div>
                <div className="text-xs text-white/80">
                  {useDirectLlm ? '🔗 Directo al LLM' : '🔗 Via ChatbotService'}
                  {isLoading && ' • Escribiendo...'}
                </div>
              </div>
            </div>
            <button
              onClick={clearChat}
              className="rounded-lg p-2 transition-colors hover:bg-white/10"
              title="Limpiar chat"
            >
              <Trash2 className="h-5 w-5" />
            </button>
          </div>

          {/* Messages */}
          <div className="flex-1 space-y-4 overflow-y-auto p-4">
            {messages.length === 0 && (
              <div className="flex h-full flex-col items-center justify-center text-gray-400">
                <Bot className="mb-4 h-16 w-16" />
                <p className="text-lg font-medium">Inicia una conversación</p>
                <p className="mt-1 text-sm">
                  Estás hablando con <strong>{selectedDealer.botName}</strong> de{' '}
                  <strong>{selectedDealer.name}</strong>
                </p>
                <p className="mt-2 text-xs">
                  Usa las &quot;Pruebas Rápidas&quot; del panel izquierdo o escribe tu mensaje
                </p>
              </div>
            )}

            {messages.map(msg => (
              <div
                key={msg.id}
                className={`flex ${msg.role === 'user' ? 'justify-end' : 'justify-start'}`}
              >
                <div
                  className={`max-w-[80%] rounded-2xl px-4 py-3 ${
                    msg.role === 'user'
                      ? 'bg-green-600 text-white'
                      : msg.isFallback
                        ? 'border border-red-200 bg-red-50 text-red-800'
                        : 'bg-gray-100 text-gray-800'
                  }`}
                >
                  {/* Sender */}
                  <div className="mb-1 flex items-center gap-1.5">
                    {msg.role === 'user' ? (
                      <User className="h-3.5 w-3.5" />
                    ) : (
                      <Bot className="h-3.5 w-3.5" />
                    )}
                    <span className="text-xs font-medium opacity-70">
                      {msg.role === 'user' ? 'Tú' : selectedDealer.botName}
                    </span>
                    <span className="text-xs opacity-50">
                      {msg.timestamp.toLocaleTimeString('es-DO', {
                        hour: '2-digit',
                        minute: '2-digit',
                      })}
                    </span>
                  </div>

                  {/* Content */}
                  <p className="text-sm whitespace-pre-wrap">{msg.content}</p>

                  {/* Quick Replies */}
                  {msg.quickReplies && msg.quickReplies.length > 0 && (
                    <div className="mt-2 flex flex-wrap gap-1.5">
                      {msg.quickReplies.map((qr, i) => (
                        <button
                          key={i}
                          onClick={() => {
                            setInput(qr);
                            inputRef.current?.focus();
                          }}
                          className="rounded-full border border-green-300 bg-white px-3 py-1 text-xs text-green-700 transition-colors hover:bg-green-50"
                        >
                          {qr}
                        </button>
                      ))}
                    </div>
                  )}

                  {/* Bot metadata */}
                  {msg.role === 'assistant' && !msg.isFallback && (
                    <div className="mt-2 flex flex-wrap gap-2 text-xs opacity-60">
                      {msg.intentName && (
                        <span className="rounded bg-gray-200 px-2 py-0.5">🎯 {msg.intentName}</span>
                      )}
                      {msg.confidenceScore !== undefined && (
                        <span className="rounded bg-gray-200 px-2 py-0.5">
                          📊 {(msg.confidenceScore * 100).toFixed(0)}%
                        </span>
                      )}
                      {msg.responseTimeMs !== undefined && (
                        <span className="rounded bg-gray-200 px-2 py-0.5">
                          ⏱️ {msg.responseTimeMs}ms
                        </span>
                      )}
                      {msg.tokensUsed !== undefined && (
                        <span className="rounded bg-gray-200 px-2 py-0.5">
                          🔤 {msg.tokensUsed} tokens
                        </span>
                      )}
                    </div>
                  )}

                  {/* Lead Signals */}
                  {msg.parsedResponse?.leadSignals && (
                    <div className="mt-2 text-xs">
                      {Object.entries(msg.parsedResponse.leadSignals)
                        .filter(([, v]) => v)
                        .map(([k]) => (
                          <span
                            key={k}
                            className="mr-1 mb-1 inline-block rounded bg-yellow-100 px-2 py-0.5 text-yellow-700"
                          >
                            🔔 {k}
                          </span>
                        ))}
                    </div>
                  )}

                  {/* Suggested Action */}
                  {msg.parsedResponse?.suggestedAction && (
                    <div className="mt-2 rounded bg-blue-100 px-2 py-1 text-xs text-blue-700">
                      ⚡ Action: {msg.parsedResponse.suggestedAction}
                    </div>
                  )}

                  {/* Raw JSON (toggle) */}
                  {showRawJson && msg.rawJson && (
                    <details className="mt-3">
                      <summary className="cursor-pointer text-xs text-gray-500 hover:text-gray-700">
                        📋 Ver JSON raw
                      </summary>
                      <pre className="mt-1 max-h-60 overflow-x-auto overflow-y-auto rounded-lg bg-gray-900 p-3 text-xs text-green-400">
                        {msg.rawJson}
                      </pre>
                    </details>
                  )}
                </div>
              </div>
            ))}

            {/* Loading indicator */}
            {isLoading && (
              <div className="flex justify-start">
                <div className="rounded-2xl bg-gray-100 px-4 py-3">
                  <div className="flex items-center gap-2">
                    <Bot className="h-3.5 w-3.5 text-gray-500" />
                    <span className="text-xs text-gray-500">
                      {selectedDealer.botName} está escribiendo
                    </span>
                  </div>
                  <div className="mt-2 flex gap-1">
                    <div className="h-2 w-2 animate-bounce rounded-full bg-gray-400 [animation-delay:0ms]" />
                    <div className="h-2 w-2 animate-bounce rounded-full bg-gray-400 [animation-delay:150ms]" />
                    <div className="h-2 w-2 animate-bounce rounded-full bg-gray-400 [animation-delay:300ms]" />
                  </div>
                </div>
              </div>
            )}

            <div ref={messagesEndRef} />
          </div>

          {/* Input area */}
          <div className="border-t p-4">
            {llmError && (
              <div className="mb-3 flex items-center gap-2 rounded-lg bg-red-50 px-3 py-2 text-xs text-red-600">
                ⚠️ {llmError}
                <button onClick={checkLlmHealth} className="underline">
                  Reintentar
                </button>
              </div>
            )}
            <div className="flex gap-2">
              <textarea
                ref={inputRef}
                value={input}
                onChange={e => setInput(e.target.value)}
                onKeyDown={handleKeyDown}
                placeholder={`Escribe un mensaje para ${selectedDealer.botName}...`}
                className="flex-1 resize-none rounded-xl border border-gray-300 px-4 py-3 text-sm focus:border-transparent focus:ring-2 focus:ring-green-500 focus:outline-none"
                rows={1}
                disabled={isLoading}
              />
              <button
                onClick={handleSend}
                disabled={!input.trim() || isLoading}
                className="flex items-center gap-2 rounded-xl bg-green-600 px-4 py-3 text-white transition-colors hover:bg-green-700 disabled:cursor-not-allowed disabled:bg-gray-300"
              >
                <Send className="h-4 w-4" />
              </button>
            </div>
            <div className="mt-2 flex items-center justify-between text-xs text-gray-400">
              <span>Enter para enviar • Shift+Enter para nueva línea</span>
              <span>{messages.filter(m => m.role === 'assistant').length} respuestas</span>
            </div>
          </div>
        </main>
      </div>
    </div>
  );
}
