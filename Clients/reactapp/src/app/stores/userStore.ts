import { makeAutoObservable, runInAction } from "mobx";
import { history } from "../..";
import agent from "../api/agent";
import { LoginDTO,  RefreshTokenDTO,  RegisterDTO, TokenDTO, User } from "../models/user";
import { store } from "./store";

export default class UserStore {
  user: User | null = null;
  tokenDTO : TokenDTO | null = null;
  refreshTokenDTO : RefreshTokenDTO | null = null;
  refreshTokenTimeout: any;

  constructor() {
    makeAutoObservable(this);
  }

  get isLoggedIn() {
    return !!this.user;
  }

  login = async (creds: LoginDTO) => {
    try {
      const tokenDTO = await agent.Account.login(creds);
      store.commonStore.setToken(tokenDTO.accessToken);
      this.startRefreshTokenTimer(tokenDTO);
      runInAction(() => (this.tokenDTO = tokenDTO));
      history.push("/activities");
      store.modalStore.closeModal();
    } catch (error) {
      throw error;
    }
  };

  logout = () => {
    store.commonStore.setToken(null);
    window.localStorage.removeItem("jwt");
    this.user = null;
    history.push("/");
  };

  register = async (registerDTO: RegisterDTO) => {
    try {
      await agent.Account.register(registerDTO);
      /*store.commonStore.setToken(user.token);
      this.startRefreshTokenTimer(user);
      runInAction(() => (this.user = user));
      history.push("/activities");*/
      history.push(`/account/registerSuccess?email=${registerDTO.email}`);
      store.modalStore.closeModal();
    } catch (error) {
      throw error;
    }
  }; 

  refreshToken = async (token :RefreshTokenDTO) => {
    this.stopRefreshTokenTimer();
    try {
      const refreshToken = await agent.Account.refreshToken(token);
      runInAction(() => (this.refreshTokenDTO = refreshToken));      
      store.commonStore.setToken(refreshToken.refreshToken);     

      let emptyTokenDTO : TokenDTO = {
        accessToken : refreshToken.refreshToken,
        tokenType: "",
        expiresIn: 0,
        refreshToken: ""
      }

      this.startRefreshTokenTimer(emptyTokenDTO);
    } catch (error) {
      console.log(error);
    }
  };

  private startRefreshTokenTimer(tokenDTO: TokenDTO) {

    const jwtToken = JSON.parse(atob(tokenDTO.accessToken.split(".")[1]));
    const expires = new Date(jwtToken.exp * 1000);
    const timeout = expires.getTime() - Date.now() - 60 * 1000;
    this.refreshTokenTimeout = setTimeout(this.refreshTokenTimeout, timeout);
  }

  private stopRefreshTokenTimer() {
    clearTimeout(this.refreshTokenTimeout);
  }
}
