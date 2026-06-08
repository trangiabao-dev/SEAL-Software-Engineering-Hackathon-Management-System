import React from "react";
import { Zap, MapPin, Mail, Phone, Facebook, Github, Youtube, Twitter } from "lucide-react";
import { AuthButtons } from "./AuthButtons";

const quickLinks = ["Home", "Timeline", "Prizes", "FAQ", "Sponsors"];

const socialLinks = [
  { icon: Facebook, href: "https://facebook.com", label: "Facebook" },
  { icon: Github, href: "https://github.com", label: "GitHub" },
  { icon: Youtube, href: "https://youtube.com", label: "YouTube" },
  { icon: Twitter, href: "https://twitter.com", label: "Twitter" },
];

export default function Footer({ onLoginClick, onRegisterClick }) {
  return (
    <footer className="relative bg-[#06080C] border-t border-white/[0.06]">
      <div className="max-w-7xl mx-auto px-6 py-16">
        <div className="grid md:grid-cols-4 gap-10 mb-12">
          {/* Brand col */}
          <div className="md:col-span-1 space-y-4">
            <div className="flex items-center gap-2.5">
              <div className="w-7 h-7 rounded-lg bg-[#F26F21] flex items-center justify-center">
                <Zap className="w-3.5 h-3.5 text-white" />
              </div>
              <span
                className="text-white font-extrabold text-base tracking-wider"
                style={{ fontFamily: "Montserrat, sans-serif" }}
              >
                FPTU <span className="text-[#F26F21]">Hackathon</span>
              </span>
            </div>
            <p className="text-slate-400 text-xs md:text-sm leading-relaxed">
              The most exciting 48-hour coding competition at FPT
              University, Vietnam.
            </p>
            <div className="space-y-0.5">
              <p
                className="text-[10px] font-bold tracking-widest uppercase text-[#F26F21]"
                style={{ fontFamily: "Montserrat, sans-serif" }}
              >
                FPT University
              </p>
              <p className="text-[11px] text-slate-500">
                Ho Chi Minh City Campus
              </p>
            </div>
          </div>

          {/* Quick links */}
          <div>
            <h4
              className="text-white font-bold uppercase tracking-wider text-xs mb-5"
              style={{ fontFamily: "Montserrat, sans-serif" }}
            >
              Quick Links
            </h4>
            <ul className="flex flex-col gap-3">
              {quickLinks.map((link) => (
                <li key={link}>
                  <a
                    href={`#${link.toLowerCase()}`}
                    className="text-slate-400 hover:text-[#F26F21] text-xs md:text-sm transition-colors duration-200 flex items-center gap-2 group"
                  >
                    <span className="w-1 h-1 rounded-full bg-[#F26F21] opacity-0 group-hover:opacity-100 transition-opacity" />
                    {link}
                  </a>
                </li>
              ))}
            </ul>
          </div>

          {/* Contact */}
          <div>
            <h4
              className="text-white font-bold uppercase tracking-wider text-xs mb-5"
              style={{ fontFamily: "Montserrat, sans-serif" }}
            >
              Contact Us
            </h4>
            <ul className="flex flex-col gap-4">
              <li className="flex items-start gap-3 text-xs md:text-sm text-slate-400">
                <MapPin className="w-4 h-4 text-[#F26F21]/80 flex-shrink-0 mt-0.5" />
                <span>
                  Lot E2a-7, D1 Street, Long Thanh My, Thu Duc, Ho Chi Minh City
                </span>
              </li>
              <li className="flex items-center gap-3 text-xs md:text-sm text-slate-400">
                <Mail className="w-4 h-4 text-[#F26F21]/80 flex-shrink-0" />
                <a
                  href="mailto:hackathon@fpt.edu.vn"
                  className="hover:text-[#F26F21] transition-colors"
                >
                  hackathon@fpt.edu.vn
                </a>
              </li>
              <li className="flex items-center gap-3 text-xs md:text-sm text-slate-400">
                <Phone className="w-4 h-4 text-[#F26F21]/80 flex-shrink-0" />
                <span>+84 28 7300 5588</span>
              </li>
            </ul>
          </div>

          {/* Social + Register CTA */}
          <div className="space-y-6">
            <div>
              <h4
                className="text-white font-bold uppercase tracking-wider text-xs mb-4"
                style={{ fontFamily: "Montserrat, sans-serif" }}
              >
                Follow Us
              </h4>
              <div className="flex gap-2">
                {socialLinks.map(({ icon: Icon, href, label }) => (
                  <a
                    key={label}
                    href={href}
                    aria-label={label}
                    className="w-8 h-8 rounded-lg bg-white/[0.02] border border-white/[0.06] flex items-center justify-center text-slate-400 hover:text-white hover:bg-white/5 hover:border-white/10 transition-all duration-200"
                  >
                    <Icon className="w-3.5 h-3.5" />
                  </a>
                ))}
              </div>
            </div>

            <div className="pt-2">
              <AuthButtons
                onLoginClick={onLoginClick}
                onRegisterClick={onRegisterClick}
                fullWidth
                className="flex-col gap-2.5"
                loginClassName="text-center w-full"
                registerClassName="text-center w-full"
              />
            </div>
          </div>
        </div>

        {/* Bottom bar */}
        <div className="border-t border-white/[0.06] pt-8 flex flex-col md:flex-row items-center justify-between gap-4">
          <p className="text-slate-500 text-xs">
            &copy; 2026 FPTU Hackathon. Organized by FPT University Student Council. All rights reserved.
          </p>
          <p className="text-slate-600 text-[10px] font-mono tracking-widest">
            CODE &middot; CREATE &middot; CONQUER
          </p>
        </div>
      </div>
    </footer>
  );
}
