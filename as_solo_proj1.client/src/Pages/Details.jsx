import { useEffect, useState } from "react";
import Detail from "../Components/Detail";

function Details() {
    const [fullname,setFullName] = useState("");
    const [phone,setPhone] = useState("");
    const [diagnosis,setDiagnosis] = useState("");
    const [plan,setPlan] = useState("");
    const [recordNumber,setRecordNumber] = useState(-1);
    const [clientId,setClientId] = useState(-1)
    const [code,setCode] = useState("");
    
    const [shouldShowDetails,setShouldShowDetails] = useState(false)
    const [error,setError] = useState("")


    useEffect(() => {
        search();
    },[]);

    const handleChange = (e) => {
        const {name,value} = e.target;
        if(name === "name") setFullName(value);
        if(name === "phone") setPhone(value);
        if(name === "diagnosis") setDiagnosis(value);
        if(name === "plan") setPlan(value);
        if(name === "record") setRecordNumber(value);
        if(name === "cid") setClientId(value);
        if(name === "code") setCode(value);
    }

    const handleSearch = async (e) => {
        e.preventDefault();
        await search();
    }

    const edit = async (e) => {
        e.preventDefault();

        const r = await fetch("/edit",{
            method: "POST",
            headers: {
                "Content-Type": "application/json",
            },
            body: JSON.stringify({
                clientID: clientId,
                accessCode: code,
                fullName: fullname,
                phoneNumber: phone,
                diagnosisDetails: diagnosis,
                treatmentPlan: plan,
                medicalRecordNumber: recordNumber
            }),
        });

        const data = await r.json();
        console.log(data);
        if(r.ok)
        {
            console.log("success");
            await search();
        }
        else {
            console.error(data.message);
            setError(data.message);
        }
    }

    const search = async () => {
        setError("");
        setShouldShowDetails(false);
        const r = await fetch("/details",{
            method: "POST",
            headers: {
                "Content-Type": "application/json",
            },
            body: JSON.stringify({
                clientID: clientId,
                accessCode: code
            }),
        });

        const data = await r.json();
        console.log(data);
        if(r.ok)
        {
            const details = data.details;
            console.log(details);
            setClientId(details.clientID);
            setDiagnosis(details.diagnosisDetails);
            setFullName(details.fullName);
            setPhone(details.phoneNumber);
            setRecordNumber(details.medicalRecordNumber);
            setPlan(details.treatmentPlan);
            setShouldShowDetails(true);
        }
        else {
            console.error(data.message);
            setError(data.message);
        }
    }

    return (
        <>
            <Detail title={"Client ID:"} id={"cid"} name={"cid"} type="number" value={clientId} handleChange={handleChange} />
            <Detail title={"Access Code:"} id={"code"} name={"code"} value={code} handleChange={handleChange} />
            <button onClick={handleSearch}>Search</button>
            <br/><br/><br/>
            {
            shouldShowDetails ? <><Detail title={"Full Name:"} id={"name"} name={"name"} value={fullname} handleChange={handleChange}/>
            <Detail title={"Phone Number:"} id={"phone"} name={"phone"} value={phone} handleChange={handleChange}/>
            <Detail title={"Diagnosis Details:"} id={"diagnosis"} name={"diagnosis"} value={diagnosis} handleChange={handleChange}/>
            <Detail title={"Treatment Plan:"} id={"plan"} name={"plan"} value={plan} handleChange={handleChange}/>
            <Detail title={"Medical Record Number:"} id={"record"} name={"record"} type="number" value={recordNumber} handleChange={handleChange}/>
                <button onClick={edit}>Save</button>
                </>
            : <></>
            }
            {error && <p className="error">{error}</p>}
        </>
    );
}

export default Details;