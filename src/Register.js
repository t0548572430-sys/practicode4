import React, { useState } from "react";
import axios from "axios";

export default function Register({ onRegister }) {
  const [username, setUsername] = useState("");
  const [password, setPassword] = useState("");

  async function register(e) {
    e.preventDefault();

    console.log("Trying to register:", { username, password });

    try {
      // ודאי שה־URL נכון
      const res = await axios.post("http://localhost:5244/register", {
        username,   // חייב להיות קטנה
        password    // חייב להיות קטנה
      });

      console.log("Register response:", res.data);
      alert("נרשמת בהצלחה!");
      onRegister("login"); // חזרה לדף הלוגין
    } catch (err) {
      console.error("Register error:", err.response?.data || err);
      alert("שגיאה בהרשמה. בדקי את הקונסול");
    }
  }

  return (
    <div className="auth-page">
      <h2>Register</h2>
      <form onSubmit={register}>
        <input
          placeholder="Username"
          value={username}
          onChange={(e) => setUsername(e.target.value)}
        />
        <input
          type="password"
          placeholder="Password"
          value={password}
          onChange={(e) => setPassword(e.target.value)}
        />
        <button>Register</button>
      </form>
      <a href="#" onClick={() => onRegister("login")}>
        יש לי כבר משתמש
      </a>
    </div>
  );
}
