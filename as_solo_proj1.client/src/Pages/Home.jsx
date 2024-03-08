import LogoutLink from "../Components/LogoutLink";
import AuthorizeView, { AuthorizedUser } from "../Components/AuthorizeView";
import Details from "./Details";

function Home() {
    return (
        <AuthorizeView>
            <span><LogoutLink>Logout <AuthorizedUser value="email" /></LogoutLink></span>
            <Details />
        </AuthorizeView>
  );
}

export default Home;