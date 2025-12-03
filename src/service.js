import axios from "axios";

axios.defaults.baseURL = "http://localhost:5244";

// הוספת ה־JWT לכל בקשה
axios.interceptors.request.use((config) => {
  const token = localStorage.getItem("token");
  if (token) {
    config.headers.Authorization = "Bearer " + token;
  }
  return config;
});

// טיפול בשגיאות והפניה ללוגין
axios.interceptors.response.use(
  (response) => response,
  (error) => {
    console.error("API ERROR:", error);

    if (error.response && error.response.status === 401) {
      console.log("401 detected → redirecting to login");
      window.location.href = "/login.html";
    }

    return Promise.reject(error);
  }
);

export default {
  getTasks: async () => {
    const result = await axios.get("/todos");
    return result.data;
  },

  addTask: async (name) => {
    const newItem = { name, isComplete: false };
    const result = await axios.post("/todos", newItem);
    return result.data;
  },

  setCompleted: async (id, isComplete) => {
    const updated = { id, name: "", isComplete };
    const result = await axios.put(`/todos/${id}`, updated);
    return result.data;
  },

  deleteTask: async (id) => {
    await axios.delete(`/todos/${id}`);
  }
};
