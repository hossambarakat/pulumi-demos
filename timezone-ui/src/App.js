import React from 'react';
import './App.css';
import TimezoneList from './TeamView';
//https://momentjs.com/data/moment-timezone-meta.json

function App() {
  const timezones = [
    {
      tz: "Australia/Sydney",
      people: [
        {
          _id: "hossam",
          name: "Hossam",
          tz: "Australia/Sydney",
          location: "Sydney"
        },
        {
          _id: "basma",
          name: "Basma",
          tz: "Australia/Sydney",
          location: "Epping"
        }
      ]
    },
    {
      tz: "Africa/Cairo",
      people: [
        {
          _id: "hazoom",
          name: "Hazoom",
          tz: "Africa/Cairo",
          location: "Cairo"
        }
      ]
    },
    {
      tz: "Pacific/Auckland",
      people: [
        {
          _id: "kiwi",
          name: "Kiwi",
          tz: "Pacific/Auckland",
          location: "Kiwi"
        }
      ]
    }
  ];
  return (
    <div className="App">
      <TimezoneList timezones={timezones}/>
    </div>
  );
}

export default App;
