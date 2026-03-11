import {
  createContext,
  useContext,
  useState,
  type PropsWithChildren,
} from "react";
import { login } from "../../lib/api";
import {
  clearStoredSession,
  getStoredSession,
  isSessionActive,
  storeSession,
  type AuthSession,
} from "./authStorage";

type AuthContextValue = {
  session: AuthSession | null;
  isAuthenticated: boolean;
  signIn: (email: string, password: string) => Promise<void>;
  signOut: () => void;
};

const AuthContext = createContext<AuthContextValue | null>(null);

export function AuthProvider({ children }: PropsWithChildren) {
  const [session, setSession] = useState<AuthSession | null>(() => {
    const stored = getStoredSession();
    return isSessionActive(stored) ? stored : null;
  });

  const value: AuthContextValue = {
    session,
    isAuthenticated: isSessionActive(session),
    async signIn(email: string, password: string) {
      const nextSession = await login({ email, password });
      storeSession(nextSession);
      setSession(nextSession);
    },
    signOut() {
      clearStoredSession();
      setSession(null);
    },
  };

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>;
}

export function useAuth() {
  const value = useContext(AuthContext);
  if (!value) {
    throw new Error("AuthContext is not available.");
  }

  return value;
}
