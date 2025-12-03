import React, { useEffect, useState } from "react";
import service from "./service.js";
import Login from "./Login.js";
import Register from "./Register.js";

function App() {
  const [page, setPage] = useState(() =>
    localStorage.getItem("token") ? "home" : "login"
  );

  const [newTodo, setNewTodo] = useState("");
  const [todos, setTodos] = useState([]);

  async function getTodos() {
    const items = await service.getTasks();
    setTodos(items);
  }

  useEffect(() => {
    if (page === "home") getTodos();
  }, [page]);

  // דפי AUTH
  if (page === "login") return <Login onLogin={(p) => setPage(p || "home")} />;
  if (page === "register") return <Register onRegister={() => setPage("login")} />;

  // דף ראשי
  return (
    <section className="todoapp">
      <header className="header">
        <h1>todos</h1>

        <button onClick={() => { localStorage.removeItem("token"); setPage("login"); }}>
          Logout
        </button>

        <form onSubmit={async (e) => {
          e.preventDefault();
          await service.addTask(newTodo);
          setNewTodo("");
          await getTodos();
        }}>
          <input className="new-todo" placeholder="Add task" value={newTodo} onChange={(e) => setNewTodo(e.target.value)} />
        </form>
      </header>

      <ul className="todo-list">
        {todos.map((todo) => (
          <li key={todo.id}>
            <input type="checkbox"
              defaultChecked={todo.isComplete}
              onChange={(e) => service.setCompleted(todo.id, e.target.checked).then(getTodos)}
            />
            <label>{todo.name}</label>
            <button onClick={() => service.deleteTask(todo.id).then(getTodos)}>X</button>
          </li>
        ))}
      </ul>
    </section>
  );
}

export default App;
