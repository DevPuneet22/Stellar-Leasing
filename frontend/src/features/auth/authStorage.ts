export type AuthSession = {
  accessToken: string;
  expiresAtUtc: string;
  email: string;
  displayName: string;
  role: string;
  tenantId: string;
};

const storageKey = "stellar-leasing.session";

export function getStoredSession(): AuthSession | null {
  const raw = window.localStorage.getItem(storageKey);
  if (!raw) {
    return null;
  }

  try {
    return JSON.parse(raw) as AuthSession;
  } catch {
    window.localStorage.removeItem(storageKey);
    return null;
  }
}

export function storeSession(session: AuthSession) {
  window.localStorage.setItem(storageKey, JSON.stringify(session));
}

export function clearStoredSession() {
  window.localStorage.removeItem(storageKey);
}

export function isSessionActive(session: AuthSession | null) {
  return session !== null && Date.parse(session.expiresAtUtc) > Date.now();
}
