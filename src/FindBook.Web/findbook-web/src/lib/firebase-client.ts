import { initializeApp, getApp, getApps, type FirebaseApp } from "firebase/app";
import { getAnalytics, isSupported, type Analytics } from "firebase/analytics";
import {
  browserLocalPersistence,
  type AuthError,
  getAuth,
  setPersistence,
  signInWithEmailAndPassword,
  signOut as firebaseSignOut,
  type Auth,
} from "firebase/auth";

import { getEnv } from "@/constants/config";

let cachedAnalyticsPromise: Promise<Analytics | null> | null = null;
let cachedAuth: Auth | null = null;
let hasLoggedFirebaseConfig = false;

export function getFirebaseApp(): FirebaseApp {
  const {
    NEXT_PUBLIC_FIREBASE_API_KEY,
    NEXT_PUBLIC_FIREBASE_APP_ID,
    NEXT_PUBLIC_FIREBASE_AUTH_DOMAIN,
    NEXT_PUBLIC_FIREBASE_MEASUREMENT_ID,
    NEXT_PUBLIC_FIREBASE_MESSAGING_SENDER_ID,
    NEXT_PUBLIC_FIREBASE_PROJECT_ID,
    NEXT_PUBLIC_FIREBASE_STORAGE_BUCKET,
  } = getEnv();

  if (getApps().length > 0) {
    return getApp();
  }

  const app = initializeApp({
    apiKey: NEXT_PUBLIC_FIREBASE_API_KEY,
    appId: NEXT_PUBLIC_FIREBASE_APP_ID,
    authDomain: NEXT_PUBLIC_FIREBASE_AUTH_DOMAIN,
    measurementId: NEXT_PUBLIC_FIREBASE_MEASUREMENT_ID,
    messagingSenderId: NEXT_PUBLIC_FIREBASE_MESSAGING_SENDER_ID,
    projectId: NEXT_PUBLIC_FIREBASE_PROJECT_ID,
    storageBucket: NEXT_PUBLIC_FIREBASE_STORAGE_BUCKET,
  });

  if (process.env.NODE_ENV !== "production" && !hasLoggedFirebaseConfig) {
    hasLoggedFirebaseConfig = true;
    console.info("[FindBook] Firebase frontend config", {
      appId: NEXT_PUBLIC_FIREBASE_APP_ID,
      authDomain: NEXT_PUBLIC_FIREBASE_AUTH_DOMAIN,
      projectId: NEXT_PUBLIC_FIREBASE_PROJECT_ID,
    });
  }

  return app;
}

export function initializeFirebaseAnalytics(): Promise<Analytics | null> {
  if (typeof window === "undefined") {
    return Promise.resolve(null);
  }

  if (!cachedAnalyticsPromise) {
    cachedAnalyticsPromise = isSupported().then((supported) => {
      if (!supported) {
        return null;
      }

      return getAnalytics(getFirebaseApp());
    });
  }

  return cachedAnalyticsPromise;
}

export async function getFirebaseAuth(): Promise<Auth> {
  if (cachedAuth) {
    return cachedAuth;
  }

  const auth = getAuth(getFirebaseApp());
  await setPersistence(auth, browserLocalPersistence);
  cachedAuth = auth;
  return auth;
}

export async function signInWithFirebase(email: string, password: string) {
  const auth = await getFirebaseAuth();

  try {
    return await signInWithEmailAndPassword(auth, email.trim(), password);
  } catch (error) {
    throw new Error(mapFirebaseAuthError(error));
  }
}

export async function signOutFromFirebase() {
  const auth = await getFirebaseAuth();
  await firebaseSignOut(auth);
}

export function getFirebaseClientDiagnostics() {
  const {
    NEXT_PUBLIC_FIREBASE_APP_ID,
    NEXT_PUBLIC_FIREBASE_AUTH_DOMAIN,
    NEXT_PUBLIC_FIREBASE_PROJECT_ID,
  } = getEnv();

  return {
    appId: NEXT_PUBLIC_FIREBASE_APP_ID,
    authDomain: NEXT_PUBLIC_FIREBASE_AUTH_DOMAIN,
    projectId: NEXT_PUBLIC_FIREBASE_PROJECT_ID,
  };
}

function mapFirebaseAuthError(error: unknown) {
  const code = (error as AuthError | undefined)?.code;

  switch (code) {
    case "auth/invalid-credential":
    case "auth/invalid-login-credentials":
      return "Firebase rejected the email or password. Double-check the credentials and confirm this frontend is pointed at the same Firebase project where the user exists.";
    case "auth/user-not-found":
      return "No Firebase user exists for this email address.";
    case "auth/wrong-password":
      return "The password is incorrect for this Firebase user.";
    case "auth/invalid-email":
      return "The email address format is invalid.";
    case "auth/too-many-requests":
      return "Firebase temporarily blocked sign-in attempts for this account. Wait a bit and try again.";
    case "auth/network-request-failed":
      return "The browser could not reach Firebase. Check your connection and try again.";
    default:
      return error instanceof Error ? error.message : "Unable to sign in with Firebase.";
  }
}
