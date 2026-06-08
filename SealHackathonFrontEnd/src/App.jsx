import React from "react";
import "./index.css";
import {
  createBrowserRouter,
  RouterProvider,
  Navigate,
} from "react-router-dom";
import { useSelector } from "react-redux";

import Home from "./page/Home";
import VerifyEmail from "./page/VerifyEmail";
import UserDashboard from "./page/UserDashboard";
import CoordinatorDashboardUI from "./page/CoordinatorDashboardUI";
import MentorDashboard from "./page/MentorDashboard";
import JudgeDashboard from "./page/JudgeDashboard";
import ResultPage from "./page/ResultPage";

// ---- Error pages ----
import Pending from "./page/error/Pending";
import Rejected from "./page/error/Decline";
import NotFound from "./page/error/NotFoundPage";
import Forbidden from "./page/error/NotPermissionPage";

// ---- Role → dashboard map ----
const ROLE_DASHBOARD = {
  Leader: "/dashboard",
  Coordinator: "/coordinator/dashboard",
  Mentor: "/mentor/dashboard",
  Judge: "/judge/dashboard",
};

// ---- Public Route — redirect nếu đã đăng nhập ----
function PublicRoute({ children }) {
  const { isAuthenticated, user } = useSelector((s) => s.auth);
  if (!isAuthenticated) return children;
  if (user?.systemRole === "Pending") return <Navigate to="/pending" replace />;
  if (user?.systemRole === "Rejected")
    return <Navigate to="/rejected" replace />;
  const dest = ROLE_DASHBOARD[user?.systemRole];
  return dest ? <Navigate to={dest} replace /> : children;
}

// ---- Protected Route ----
function ProtectedRoute({ children, allowedRoles }) {
  const { isAuthenticated, user } = useSelector((s) => s.auth);

  if (!isAuthenticated) return <Navigate to="/" replace />;

  if (user?.systemRole === "Pending") return <Navigate to="/pending" replace />;
  if (user?.systemRole === "Rejected")
    return <Navigate to="/rejected" replace />;

  if (allowedRoles && !allowedRoles.includes(user?.systemRole))
    return <Navigate to="/403" replace />;

  return children;
}

// ---- Routes ----
const router = createBrowserRouter([
  {
    path: "/",
    element: (
      <PublicRoute>
        <Home />
      </PublicRoute>
    ),
  },
  {
    path: "/login",
    element: (
      <PublicRoute>
        <Home defaultLoginOpen />
      </PublicRoute>
    ),
  },
  {
    path: "/register",
    element: (
      <PublicRoute>
        <Home defaultRegisterOpen />
      </PublicRoute>
    ),
  },
  { path: "/verify-email", element: <VerifyEmail /> },
  { path: "/pending", element: <Pending /> },
  { path: "/rejected", element: <Rejected /> },
  { path: "/403", element: <Forbidden /> },
  { path: "*", element: <NotFound /> },

  {
    path: "/dashboard",
    element: (
      <ProtectedRoute allowedRoles={["Leader"]}>
        <UserDashboard />
      </ProtectedRoute>
    ),
  },
  {
    path: "/coordinator/dashboard",
    element: (
      <ProtectedRoute allowedRoles={["Coordinator"]}>
        <CoordinatorDashboardUI />
      </ProtectedRoute>
    ),
  },
  {
    path: "/mentor/dashboard",
    element: (
      <ProtectedRoute allowedRoles={["Mentor"]}>
        <MentorDashboard />
      </ProtectedRoute>
    ),
  },
  {
    path: "/judge/dashboard",
    element: (
      <ProtectedRoute allowedRoles={["Judge"]}>
        <JudgeDashboard />
      </ProtectedRoute>
    ),
  },
  { path: "/results", element: <ResultPage /> },
]);

export default function App() {
  return (
    <div className="min-h-screen bg-[#0B0E14]">
      <RouterProvider router={router} />
    </div>
  );
}
