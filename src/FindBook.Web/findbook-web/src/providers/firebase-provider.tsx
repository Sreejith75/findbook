"use client";

import type { ReactNode } from "react";
import { createContext, useEffect, useState } from "react";
import type { User } from "firebase/auth";
import { onIdTokenChanged } from "firebase/auth";

import { AUTH_COOKIE_NAME } from "@/constants/auth";
import { resetAuthenticatedSessionCache } from "@/features/book-rental/services/book-rental.client";
import {
  getFirebaseAuth,
  initializeFirebaseAnalytics,
  signInWithFirebase,
  signOutFromFirebase,
} from "@/lib/firebase-client";

type FirebaseProviderProps = {
  children: ReactNode;
};

type AuthContextValue = {
  isAuthenticated: boolean;
  isLoading: boolean;
  signIn: (email: string, password: string) => Promise<void>;
  signOut: () => Promise<void>;
  user: User | null;
};

export const AuthContext = createContext<AuthContextValue | null>(null);

export function FirebaseProvider({ children }: FirebaseProviderProps) {
  const [user, setUser] = useState<User | null>(null);
  const [isLoading, setIsLoading] = useState(true);

  useEffect(() => {
    void initializeFirebaseAnalytics();
  }, []);

  useEffect(() => {
    let isMounted = true;

    const setup = async () => {
      const auth = await getFirebaseAuth();

      return onIdTokenChanged(auth, async (nextUser) => {
        if (!isMounted) {
          return;
        }

        setUser(nextUser);

        if (nextUser) {
          const token = await nextUser.getIdToken();
          document.cookie = `${AUTH_COOKIE_NAME}=${token}; Path=/; SameSite=Lax`;
        } else {
          document.cookie = `${AUTH_COOKIE_NAME}=; Path=/; Max-Age=0; SameSite=Lax`;
        }

        resetAuthenticatedSessionCache();
        setIsLoading(false);
      });
    };

    let unsubscribe: (() => void) | undefined;

    void setup().then((cleanup) => {
      unsubscribe = cleanup;
    });

    return () => {
      isMounted = false;
      unsubscribe?.();
    };
  }, []);

  const value: AuthContextValue = {
    isAuthenticated: !!user,
    isLoading,
    signIn: async (email: string, password: string) => {
      const credential = await signInWithFirebase(email, password);
      const token = await credential.user.getIdToken(true);
      document.cookie = `${AUTH_COOKIE_NAME}=${token}; Path=/; SameSite=Lax`;
      resetAuthenticatedSessionCache();
    },
    signOut: async () => {
      await signOutFromFirebase();
      document.cookie = `${AUTH_COOKIE_NAME}=; Path=/; Max-Age=0; SameSite=Lax`;
      resetAuthenticatedSessionCache();
    },
    user,
  };

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>;
}
