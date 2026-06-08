import api from "./axiosInstance";

const memberService = {
  addMember: (teamId, data) => api.post(`/api/teams/${teamId}/members`, data),
  updateMember: (teamId, memberId, data) =>
    api.put(`/api/teams/${teamId}/members/${memberId}`, data),
  deleteMember: (teamId, memberId) =>
    api.delete(`/api/teams/${teamId}/members/${memberId}`),
};

export default memberService;
