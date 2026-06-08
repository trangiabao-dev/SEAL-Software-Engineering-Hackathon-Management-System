import api from "./axiosInstance";

const topicService = {
  getByRound: (roundId) => api.get(`/api/rounds/${roundId}/topics`),
  create: (roundId, data) => api.post(`/api/rounds/${roundId}/topics`, data),
};

export default topicService;
