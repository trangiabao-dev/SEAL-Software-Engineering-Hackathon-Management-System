import api from "./axiosInstance";

const mentorService = {
  getAll: (params) => api.get("/api/mentors", { params }),
};

export default mentorService;
