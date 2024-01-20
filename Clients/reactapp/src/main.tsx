import React from 'react'
import ReactDOM from 'react-dom/client'
import App from './App.tsx'
import './index.css'
import { Router } from 'react-router-dom';
import { StoreContext, store } from './app/stores/store.ts';
import ScrollToTop from './app/layout/ScrollToTop.tsx';
import {createBrowserHistory} from 'history';

export const history = createBrowserHistory();

ReactDOM.render(
  <StoreContext.Provider value={store}>
    <Router history={history}>
      <ScrollToTop/>
      <App />
    </Router>
  </StoreContext.Provider>,
  document.getElementById('root')
);
