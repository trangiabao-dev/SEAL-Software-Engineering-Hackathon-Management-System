import api from "./axiosInstance";

const eventService = {
  getAll: () => api.get("/api/events"),
  getById: (id) => api.get(`/api/events/${id}`),
  create: (data) => api.post("/api/events", data),
  update: (id, data) => api.put(`/api/events/${id}`, data),
  remove: (id) => api.delete(`/api/events/${id}`),
};

export default eventService;
