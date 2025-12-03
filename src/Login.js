import React, { useState } from "react";
import axios from "axios";

export default function Login({ onLogin }) {
  const [username, setUsername] = useState("");
  const [password, setPassword] = useState("");

  async function login(e) {
    e.preventDefault();
    try {
      const res = await axios.post("/login", { username, password });
      const token = res.data.token;

      localStorage.setItem("token", token);

      onLogin(); // חזרה לדף הראשי
    } catch {
      alert("שם משתמש או סיסמה שגויים");
    }
  }

  return (
    <div className="auth-page">
      <h2>Login</h2>
      <form onSubmit={login}>
        <input placeholder="Username" value={username} onChange={(e) => setUsername(e.target.value)} />
        <input type="password" placeholder="Password" value={password} onChange={(e) => setPassword(e.target.value)} />
        <button>Login</button>
      </form>
      <a href="#" onClick={() => onLogin("register")}>או הרשמה</a>
    </div>
  );
}
