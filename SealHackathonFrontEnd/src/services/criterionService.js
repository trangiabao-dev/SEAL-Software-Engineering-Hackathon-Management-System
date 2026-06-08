import api from "./axiosInstance";

const criterionService = {
  getByRound: (roundId) => api.get(`/api/rounds/${roundId}/criteria`),
  create: (roundId, data) =>
    api.post(`/api/rounds/${roundId}/criteria`, data),
  importTemplate: (roundId, templateId) =>
    api.post(`/api/rounds/${roundId}/criteria/import`, { templateId }),
  getTemplates: () => api.get("/api/criterion-templates"),
  createTemplate: (data) => api.post("/api/criterion-templates", data),
  deleteTemplate: (id) => api.delete(`/api/criterion-templates/${id}`),
};

export default criterionService;
