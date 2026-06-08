import api from "./axiosInstance";

const teamService = {
  createTeam: (data) => api.post("/api/teams", data),
  getMyTeam: () => api.get("/api/teams/my-team"), // TODO: khi BE có
  getById: (id) => api.get(`/api/teams/${id}`),
  updateTeam: (id, data) => api.put(`/api/teams/${id}`, data),

  getAdminTeams: (params) => api.get("/api/admin/teams", { params }),
  approveTeam: (id) => api.put(`/api/teams/${id}/approve`),
  disqualifyTeam: (id, reason) =>
    api.put(`/api/teams/${id}/disqualify`, { reason }),
  assignMentor: (id, mentorId) =>
    api.put(`/api/teams/${id}/mentor`, { mentorId }),

  addMember: (teamId, data) => api.post(`/api/teams/${teamId}/members`, data),
  updateMember: (teamId, memberId, data) =>
    api.put(`/api/teams/${teamId}/members/${memberId}`, data),
  deleteMember: (teamId, memberId) =>
    api.delete(`/api/teams/${teamId}/members/${memberId}`),
};

export default teamService;
