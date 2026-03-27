export type ApiRequestOptions = {
  headers?: Record<string, string>;
  next?: {
    revalidate?: number | false;
    tags?: string[];
  };
};

export type ApiError = {
  message: string;
  status: number;
  url: string;
};
