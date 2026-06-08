import { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import Navbar from '../components/Home/Navbar';
import Hero from '../components/Home/Hero';
import About from '../components/Home/About';
import Timeline from '../components/Home/Timeline';
import Prizes from '../components/Home/Prizes';
import Sponsors from '../components/Home/Sponsors';
import FAQ from '../components/Home/FAQ';
import Footer from '../components/Home/Footer';
import LoginModal from '../components/Home/LoginModal';
import RegisterModal from '../components/Home/RegisterModal';

export default function Home({ defaultLoginOpen = false, defaultRegisterOpen = false }) {
  const navigate = useNavigate();
  const [loginOpen, setLoginOpen] = useState(defaultLoginOpen);
  const [registerOpen, setRegisterOpen] = useState(defaultRegisterOpen);

  const openLogin = () => {
    setRegisterOpen(false);
    setLoginOpen(true);
    navigate('/login');
  };

  const openRegister = () => {
    setLoginOpen(false);
    setRegisterOpen(true);
    navigate('/register');
  };

  const closeLogin = () => {
    setLoginOpen(false);
    navigate('/');
  };

  const closeRegister = () => {
    setRegisterOpen(false);
    navigate('/');
  };

  return (
    <>
      <div className="min-h-screen bg-[#080A0F]">
        <Navbar onLoginClick={openLogin} onRegisterClick={openRegister} />
        <main>
          <Hero onRegisterClick={openRegister} />
          <About />
          <Timeline />
          <Prizes />
          <Sponsors />
          <FAQ />
        </main>
        <Footer onLoginClick={openLogin} onRegisterClick={openRegister} />
      </div>

      <LoginModal open={loginOpen} onClose={closeLogin} />
      <RegisterModal
        open={registerOpen}
        onClose={closeRegister}
      />
    </>
  );
}
