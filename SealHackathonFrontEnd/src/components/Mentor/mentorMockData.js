export const mentorProfile = {
  id: "mentor-01",
  name: "Linh Tran",
  role: "Mentor",
  expertise: "React, UX Research",
  activeEvent: "FPT Hackathon 2026",
};

export const activeEvent = {
  name: "FPT Hackathon 2026",
  status: "Active",
  location: "FPT University HCMC",
  timeline: "May 24 - May 26, 2026",
};

export const currentRound = {
  name: "Prototype Submission",
  status: "Open",
  window: "Round 2",
  closesIn: "36 hours",
};

export const assignedTracks = [
  {
    id: "track-web",
    name: "Web Application",
    description: "Full-stack web products for campus and education workflows.",
    teams: 3,
    maxTeamsPerMentor: 3,
    topics: 3,
    status: "At capacity",
  },
  {
    id: "track-ai",
    name: "AI Solution",
    description: "Applied AI prototypes with responsible user-centered design.",
    teams: 1,
    maxTeamsPerMentor: 3,
    topics: 1,
    status: "Available",
  },
];

export const mentorTeams = [
  {
    id: "team-nova",
    teamName: "Team Nova",
    university: "FPT University HCMC",
    track: "Web Application",
    status: "Approved",
    progress: 84,
    activity: "Submitted demo link 18 minutes ago",
    members: [
      { id: "mem-01", fullName: "An Nguyen", studentCode: "SE170001", isFPTStudent: true, isLeader: true },
      { id: "mem-02", fullName: "Bao Tran", studentCode: "SE170114", isFPTStudent: true, isLeader: false },
      { id: "mem-03", fullName: "Chi Le", studentCode: "AI170219", isFPTStudent: true, isLeader: false },
      { id: "mem-04", fullName: "Duc Pham", studentCode: "UX240018", isFPTStudent: false, isLeader: false },
    ],
    submission: {
      status: "Submitted",
      demoUrl: "https://example.com/team-nova-demo",
      reportUrl: "https://example.com/team-nova-report.pdf",
      submittedAt: "May 25, 2026 14:30",
      round: "Prototype Submission",
    },
    topic: {
      title: "Smart Campus Assistant",
      description: "Build a digital assistant that helps students discover rooms, deadlines, services, and academic resources across campus.",
      requirements: ["Mobile-friendly experience", "Role-aware information cards", "Searchable campus service knowledge base"],
      attachments: [
        { label: "Topic brief", url: "https://example.com/topics/smart-campus-brief.pdf" },
      ],
    },
  },
  {
    id: "byte-builders",
    teamName: "Byte Builders",
    university: "FPT University Can Tho",
    track: "AI Solution",
    status: "Pending",
    progress: 52,
    activity: "Updated report document 1 hour ago",
    members: [
      { id: "mem-05", fullName: "Hanh Vo", studentCode: "AI180045", isFPTStudent: true, isLeader: true },
      { id: "mem-06", fullName: "Khoa Dang", studentCode: "AI180119", isFPTStudent: true, isLeader: false },
      { id: "mem-07", fullName: "Minh Ho", studentCode: "SE180331", isFPTStudent: true, isLeader: false },
    ],
    submission: {
      status: "In progress",
      demoUrl: "https://example.com/byte-builders-demo",
      reportUrl: "https://example.com/byte-builders-report.pdf",
      submittedAt: "Not submitted",
      round: "Prototype Submission",
    },
    topic: {
      title: "Learning Path Recommender",
      description: "Create an AI-assisted recommendation prototype for personalized software engineering learning plans.",
      requirements: ["Explainable recommendation output", "Student profile input", "Ethical use disclosure"],
      attachments: [
        { label: "Dataset guide", url: "https://example.com/topics/learning-path-data.pdf" },
      ],
    },
  },
  {
    id: "code-catalysts",
    teamName: "Code Catalysts",
    university: "FPT University HCMC",
    track: "Web Application",
    status: "Approved",
    progress: 72,
    activity: "Viewed mentor feedback checklist today",
    members: [
      { id: "mem-08", fullName: "Nhi Bui", studentCode: "SE171222", isFPTStudent: true, isLeader: true },
      { id: "mem-09", fullName: "Quang Ly", studentCode: "SE171317", isFPTStudent: true, isLeader: false },
      { id: "mem-10", fullName: "Tuan Do", studentCode: "GD240077", isFPTStudent: false, isLeader: false },
    ],
    submission: {
      status: "Submitted",
      demoUrl: "https://example.com/code-catalysts-demo",
      reportUrl: "https://example.com/code-catalysts-report.pdf",
      submittedAt: "May 25, 2026 10:15",
      round: "Prototype Submission",
    },
    topic: {
      title: "Event Check-in Portal",
      description: "Design a streamlined check-in and attendance dashboard for academic technology events.",
      requirements: ["QR check-in flow", "Attendance summary", "Responsive dashboard"],
      attachments: [],
    },
  },
  {
    id: "syntax-squad",
    teamName: "Syntax Squad",
    university: "University of Science",
    track: "Web Application",
    status: "Disqualified",
    progress: 36,
    activity: "No submission activity in current round",
    members: [
      { id: "mem-11", fullName: "Long Phan", studentCode: "HCMUS001", isFPTStudent: false, isLeader: true },
      { id: "mem-12", fullName: "My Nguyen", studentCode: "HCMUS118", isFPTStudent: false, isLeader: false },
    ],
    submission: {
      status: "Missing",
      demoUrl: "",
      reportUrl: "",
      submittedAt: "Not submitted",
      round: "Prototype Submission",
    },
    topic: {
      title: "Academic Club Marketplace",
      description: "Prototype a platform for students to discover, join, and coordinate academic club projects.",
      requirements: ["Club discovery", "Project request board", "Member interest tracking"],
      attachments: [
        { label: "Reference workflow", url: "https://example.com/topics/club-marketplace.pdf" },
      ],
    },
  },
];

export const recentTeamActivity = mentorTeams.map((team) => ({
  id: `activity-${team.id}`,
  team: team.teamName,
  action: team.activity,
  status: team.submission.status,
}));
