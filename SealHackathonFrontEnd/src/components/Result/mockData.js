// Mock data for the Result Announcement Page

// Announcement time: set to a near-future time for testing the locked state.
// Change this to a past date to see the published state.
export const announcementTime = new Date('2026-03-17T14:00:00');

// The current user's team ID (for row highlighting in the leaderboard)
export const currentUserTeamId = 'team-004';

// Event-level statistics
export const eventStats = {
  totalParticipants: 487,
  totalSubmissions: 92,
  tracksCompeted: 6,
  judgesInvolved: 24,
};

// Ranked teams (ordered by final rank)
export const teams = [
  {
    id: 'team-001',
    rank: 1,
    name: 'Neural Nexus',
    project: 'AI-Powered Campus Safety System',
    track: 'Artificial Intelligence',
    score: 96.4,
    members: ['Minh Tran', 'Linh Nguyen', 'Khoa Pham', 'Thao Le'],
    avatar: 'NN',
  },
  {
    id: 'team-002',
    rank: 2,
    name: 'ByteBuilders',
    project: 'Real-Time Code Collaboration Platform',
    track: 'Developer Tools',
    score: 93.8,
    members: ['Duc Vo', 'Hoa Mai', 'Tung Do', 'Anh Bui'],
    avatar: 'BB',
  },
  {
    id: 'team-003',
    rank: 3,
    name: 'GreenStack',
    project: 'Carbon Footprint Tracker for Universities',
    track: 'Sustainability',
    score: 91.2,
    members: ['Nam Hoang', 'Yen Tran', 'Phuc Le'],
    avatar: 'GS',
  },
  {
    id: 'team-004',
    rank: 4,
    name: 'Team Alpha',
    project: 'Smart Student Health Dashboard',
    track: 'Health Tech',
    score: 89.5,
    members: ['Quang Nguyen', 'My Phan', 'Vinh Dang', 'Loan Truong'],
    avatar: 'TA',
  },
  {
    id: 'team-005',
    rank: 5,
    name: 'CloudCraft',
    project: 'Serverless Event Management Platform',
    track: 'Cloud Computing',
    score: 87.1,
    members: ['Hai Le', 'Thu Pham', 'Long Vu'],
    avatar: 'CC',
  },
  {
    id: 'team-006',
    rank: 6,
    name: 'DataDrivenX',
    project: 'Predictive Course Enrollment Analytics',
    track: 'Data Science',
    score: 85.6,
    members: ['Binh Tran', 'Nhi Doan', 'Khanh Nguyen', 'Duy Ho'],
    avatar: 'DX',
  },
  {
    id: 'team-007',
    rank: 7,
    name: 'PixelPioneers',
    project: 'AR Campus Navigation App',
    track: 'Mobile Development',
    score: 83.9,
    members: ['Tan Le', 'Huong Vu', 'Son Pham'],
    avatar: 'PP',
  },
  {
    id: 'team-008',
    rank: 8,
    name: 'SecureShield',
    project: 'Zero-Trust Network Access for Education',
    track: 'Cybersecurity',
    score: 81.3,
    members: ['Trung Nguyen', 'Ngoc Bui', 'Dat Tran', 'Van Le'],
    avatar: 'SS',
  },
  {
    id: 'team-009',
    rank: 9,
    name: 'EduFlow',
    project: 'Gamified Learning Management System',
    track: 'EdTech',
    score: 79.7,
    members: ['Phuong Dang', 'Cuong Hoang', 'Uyen Tran'],
    avatar: 'EF',
  },
  {
    id: 'team-010',
    rank: 10,
    name: 'BlockBridge',
    project: 'Blockchain-Based Certificate Verification',
    track: 'Blockchain',
    score: 77.2,
    members: ['An Phan', 'Thuy Nguyen', 'Hieu Le', 'Mai Vo'],
    avatar: 'BK',
  },
];
