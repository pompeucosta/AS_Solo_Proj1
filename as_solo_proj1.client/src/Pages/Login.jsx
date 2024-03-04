import { useState } from "react";
import { useNavigate } from "react-router-dom";

function Login() {
    const [email, setEmail] = useState("");
    const [password, setPassword] = useState("");
    const [rememberMe, setRememberMe] = useState(false);

    const [error, setError] = useState("");
    const navigate = useNavigate();

    const handleChange = (e) => {
        const { name, value } = e.target;
        if (name === "email") setEmail(value);
        if (name === "password") setPassword(value);
        if (name === "rememberme") setRememberMe(e.target.checked);
    };

    const handleRegisterClick = () => {
        navigate("/register");
    }

    const handleSubmit = (e) => {
        e.preventDefault();

        if (!email || !password) {
            setError("Please fill in all fields");
        } else {
            setError("");
            var loginurl = "";
            if (rememberMe == true)
                loginurl = "/login?useCookies=true";
            else
                loginurl = "/login?useSessionCookies=true";

            fetch(loginurl, {
                method: "POST",
                headers: {
                    "Content-Type": "application/json",
                },
                body: JSON.stringify({ email: email, password: password, }),
            })
                .then((data) => {
                    console.log(data);
                    if (data.ok) {
                        setError("Successful login.");
                        window.location.href = "/";
                    }
                    else
                        setError("Error logging in.");
                })
                .catch((error) => {
                    console.error(error);
                    setError("Error logging in");
                });
        }
    };

    return (
        <div className="containerbox">
            <h3>Login</h3>
            <form onSubmit={handleSubmit}>
                <div>
                    <label className="forminput" htmlFor="email">Email:</label>
                </div>
                <div>
                    <input
                        type="email"
                        id="email"
                        name="email"
                        value={email}
                        onChange={handleChange}
                    />
                </div>
                <div>
                    <input
                        type="password"
                        id="password"
                        name="password"
                        value={password}
                        onChange={handleChange}
                    />
                </div>
                <div>
                    <input
                        type="checkbox"
                        id="rememberme"
                        name="rememberme"
                        checked={rememberMe}
                        onChange={handleChange}
                    /><span>Remember Me</span>
                </div>
                <div>
                    <button type="submit">Login</button>
                </div>
                <div>
                    <button onClick={handleRegisterClick}>Register</button>
                </div>
            </form>
            {error && <p className="error">{error}</p> }
        </div>
    )
}

export default Login;