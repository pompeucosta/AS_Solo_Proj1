import { useState } from "react";
import { useNavigate } from "react-router-dom";

function Register() {
    const [email, setEmail] = useState("");
    const [password, setPassword] = useState("");
    const [confirmPassword, setConfirmPassword] = useState("");
    const [name, setName] = useState("");
    const [phone, setPhone] = useState("");
    const [code, setCode] = useState("");
    const navigate = useNavigate();

    const [error, setError] = useState("");

    const handleLoginClick = () => {
        navigate("/login");
    }

    const handleChange = (e) => {
        const { name, value } = e.target;
        if (name === "email") setEmail(value);
        if (name === "password") setPassword(value);
        if (name === "confirmPassword") setConfirmPassword(value);
        if (name === "name") setName(value);
        if (name === "code") setCode(value);
        if (name === "phone") setPhone(value);
    };

    const handleSubmit = (e) => {
        e.preventDefault();

        if (!email || !password || !confirmPassword || !name || !phone || !code) {
            setError("Please fill in all fields");
        }
        else if (!/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(email)) {
            setError("Please enter a valid email address.");
        } else if (password !== confirmPassword) {
            setError("Passwords do not match.");
        } else if (/^\d{9}$/.test(phone)) {
            setError("Phone number must contain only digits and must have 9 digits");
        } else {
            // clear error message
            setError("");
            // post data to the /register api
            fetch("/register", {
                method: "POST",
                headers: {
                    "Content-Type": "application/json",
                },
                body: JSON.stringify({
                    name: name,
                    email: email,
                    password: password,
                    phoneNumber: phone,
                    accessCode: code
                }),
            })
                //.then((response) => response.json())
                .then((data) => {
                    // handle success or error from the server
                    console.log(data);
                    if (data.ok)
                        setError("Successful register.");
                    else
                        setError("Error registering.");

                })
                .catch((error) => {
                    // handle network error
                    console.error(error);
                    setError("Error registering.");
                });
        }
    };

    return (
        <div className="containerbox">
            <h3>Register</h3>

            <form onSubmit={handleSubmit}>
                <div>
                    <label htmlFor="name">Name:</label>
                </div><div>
                    <input
                        type="text"
                        id="name"
                        name="name"
                        value={name}
                        onChange={handleChange}
                    />
                </div>
                <div>
                    <label htmlFor="email">Email:</label>
                </div><div>
                    <input
                        type="email"
                        id="email"
                        name="email"
                        value={email}
                        onChange={handleChange}
                    />
                </div>
                <div>
                    <label htmlFor="password">Password:</label></div><div>
                    <input
                        type="password"
                        id="password"
                        name="password"
                        value={password}
                        onChange={handleChange}
                    />
                </div>
                <div>
                    <label htmlFor="confirmPassword">Confirm Password:</label></div><div>
                    <input
                        type="password"
                        id="confirmPassword"
                        name="confirmPassword"
                        value={confirmPassword}
                        onChange={handleChange}
                    />
                </div>
                <div>
                    <label htmlFor="phone">Phone Number:</label>
                </div><div>
                    <input
                        type="text"
                        id="phone"
                        name="phone"
                        value={phone}
                        onChange={handleChange}
                    />
                </div>
                <div>
                    <label htmlFor="code">Acess Code:</label>
                </div><div>
                    <input
                        type="password"
                        id="code"
                        name="code"
                        value={code}
                        onChange={handleChange}
                    />
                </div>
                <div>
                    <button type="submit">Register</button>

                </div>
                <div>
                    <button onClick={handleLoginClick}>Go to Login</button>
                </div>
            </form>

            {error && <p className="error">{error}</p>}
        </div>
    );
}

export default Register;