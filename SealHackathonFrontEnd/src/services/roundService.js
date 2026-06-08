import api from "./axiosInstance";

const roundService = {
  getByTrack: (trackId) => api.get(`/api/tracks/${trackId}/rounds`),
  create: (data) => api.post("/api/rounds", data),
  update: (id, data) => api.put(`/api/rounds/${id}`, data),
  updateStatus: (id, status) =>
    api.put(`/api/rounds/${id}/status`, { status }),
};

export default roundService;
