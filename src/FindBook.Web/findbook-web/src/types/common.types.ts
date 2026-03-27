export type ID = number;

export type Nullable<T> = T | null;

export type Address = {
  street: string;
  city: string;
  state: string;
  postalCode: string;
  country: string;
};
