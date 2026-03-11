export const config = {
  apiBaseUrl: import.meta.env.VITE_API_BASE_URL ?? "http://localhost:5080",
  tenantId:
    import.meta.env.VITE_TENANT_ID ?? "11111111-1111-1111-1111-111111111111",
};
