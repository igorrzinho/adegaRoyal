const API_BASE_URL = 'https://adega-api-m9u2.onrender.com';

export async function apiRequest<T>(
  endpoint: string,
  options: RequestInit = {}
): Promise<{ data: T | null; error: string | null; status: number }> {
  try {
    const response = await fetch(`${API_BASE_URL}${endpoint}`, {
      ...options,
      headers: {
        'Content-Type': 'application/json',
        ...options.headers,
      },
    });

    if (response.ok) {
      const data = (await response.json()) as T;
      return { data, error: null, status: response.status };
    }

    const errorBody = await response.json().catch(() => null);
    return {
      data: null,
      error: errorBody?.message || `Erro ${response.status}`,
      status: response.status,
    };
  } catch {
    return { data: null, error: 'Não foi possível conectar ao servidor.', status: 0 };
  }
}

export { API_BASE_URL };
