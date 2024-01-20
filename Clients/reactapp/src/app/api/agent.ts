import axios, { AxiosError, AxiosResponse } from "axios";
import { request } from "http";
import { toast } from "react-toastify";
import { history } from "../..";
import { PaginatedResult } from "../models/pagination";
import { LoginDTO, RefreshTokenDTO,  RegisterDTO, TokenDTO,  } from "../models/user";
import { store } from "../stores/store";

const sleep = (delay: number) => {
  return new Promise((resolve) => {
    setTimeout(resolve, delay);
  });
};

axios.defaults.baseURL = process.env.API_URL;

axios.interceptors.request.use((config) => {
  const token = store.commonStore.token;
  if (token) config.headers.Authorization = `Bearer ${token}`;
  return config;
});

axios.interceptors.response.use(
  async (response) => {
    if (process.env.NODE_ENV === "development") await sleep(1000);
    const pagination = response.headers["pagination"];
    if (pagination) {
      response.data = new PaginatedResult(response.data, JSON.parse(pagination));
      return response as AxiosResponse<PaginatedResult<any>>;
    }
    return response;
  },
  (error: AxiosError) => {
    const { data , status, config, headers } = error.response!;
    switch (status) {
      case 400:
        if (config.method === "get" && data.errors.hasOwnProperty("id")) {
          history.push("/not-found");
        }
        if (data.errors) {
          const modalStateErrors = [];
          for (const key in data.errors) {
            if (data.errors[key]) {
              modalStateErrors.push(data.errors[key]);
            }
          }
          throw modalStateErrors.flat();
        } else {
          toast.error(data);
        }
        break;
      case 401:
        if (status === 401 && headers["www-authenticate"]?.startsWith('Bearer error="invalid_token"')) {
          store.userStore.logout();
          toast.error("Session expired - please login again");
        }
        break;
      case 404:
        history.push("/not-found");
        break;
      case 500:
        store.commonStore.setServerError(data);
        history.push("/server-error");
        break;
    }
    return Promise.reject(error);
  }
);

const responseBody = <T>(response: AxiosResponse<T>) => response.data;

const requests = {
  get: <T>(url: string) => axios.get<T>(url).then(responseBody),
  post: <T>(url: string, body: {}) => axios.post<T>(url, body).then(responseBody),
  put: <T>(url: string, body: {}) => axios.put<T>(url, body).then(responseBody),
  del: <T>(url: string) => axios.delete<T>(url).then(responseBody),
};


const Account = { 
  login: (user: LoginDTO) => requests.post<TokenDTO>("login", user),
  register: (user: RegisterDTO) => requests.post<void>("register", user),
  refreshToken: (token : RefreshTokenDTO) => requests.post<RefreshTokenDTO>("refresh", token),
  verifyEmail: (userId: string, code: string, changedEmail: string) =>
    requests.post<void>(`confirmEmail?userId=${userId}&code=${code}&changedEmail=${changedEmail}`, {}),
  resendEmailConfirm: (email: string) => requests.get(`resendConfirmationEmail?email=${email}`),
};



const agent = {
  Account
};

export default agent;
