import React from "react";
import Timezone from './timezone';
import { loadMembers, addMember, deleteMember } from '../api';
import { useEffect } from "react";
import { useState } from "react";

const TimezoneList = ({ time }) => {
  const [name, setName] = useState('');
  const [country, setCountry] = useState('');
  const [timeZone, setTimeZone] = useState('');
  const [timeZones, setTimeZones] = useState([]);

  useEffect(() => {
    loadMembers()
      .then((data) => {
        setTimeZones(data);
      });
  }, []);

  const onSubmit = async () => {
    const member = {
      name,
      country,
      timeZone
    };
    await addMember(member);
    var data = await loadMembers();
    setTimeZones(data);
  };

  const onDelete = async (id) => {
    await deleteMember(id);
    var data = await loadMembers();
    setTimeZones(data);
  };

  return (
    <div>
      <div>
        <div>
          Name:
          <input type="text"
            value={name}
            onChange={(e) => setName(e.target.value)}></input>
        </div>
        <div>
          Country:
          <input type="text"
            value={country}
            onChange={(e) => setCountry(e.target.value)}></input>
        </div>
        <div>
          Timezone:
          <input type="text"
            value={timeZone}
            onChange={(e) => setTimeZone(e.target.value)}></input>
        </div>
        <div>
          <button type="submit" onClick={onSubmit}>Submit</button>
        </div>
      </div>
      <div >
        {timeZones.map(function (timezone) {
          return (
            <Timezone
              key={timezone.timeZone}
              timezone={timezone}
              time={time}
              onDelete={onDelete}
            />
          );
        })}
      </div>

    </div>
  );
}

export default TimezoneList;