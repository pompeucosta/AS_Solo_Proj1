function Detail({title,type = "text",id,name,value,handleChange}) {
    return(
        <>
            <div>
                <label htmlFor={name}>{title}</label>
            </div><div>
                <input
                    type={type}
                    id={id}
                    name={name}
                    value={value}
                    onChange={handleChange}
                />
            </div>
        </>
    )
}

export default Detail;