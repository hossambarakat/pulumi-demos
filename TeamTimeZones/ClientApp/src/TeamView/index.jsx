import React from "react";
import Timezone from './timezone';
import { loadMembers, addMember, deleteMember } from '../api';
import { useEffect } from "react";
import { useState } from "react";
import Button from '@material-ui/core/Button';
import { makeStyles } from '@material-ui/core/styles';

import TextField from '@material-ui/core/TextField';
import Select from '@material-ui/core/Select';
import FormControl from '@material-ui/core/FormControl';
import InputLabel from '@material-ui/core/InputLabel';


import { zones } from '../timezones';

const useStyles = makeStyles(theme => ({
  root: {
    '& > *': {
      margin: theme.spacing(1),
    },
  },
  formControl: {
    margin: theme.spacing(1),
    minWidth: 120,
  },
}));


const TimezoneList = ({ time }) => {
  const [name, setName] = useState('');
  const [country, setCountry] = useState('');
  const [timeZone, setTimeZone] = useState('');
  const [timeZones, setTimeZones] = useState([]);
  const classes = useStyles();


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
      <div className={classes.root}>
        <div>
          <TextField label="Name" value={name} onChange={(e) => setName(e.target.value)} />
        </div>
        <div>
          <TextField label="Country" value={country} onChange={(e) => setCountry(e.target.value)} />
        </div>
        <div>
        <FormControl className={classes.formControl}>

          <InputLabel>TimeZone</InputLabel>
          <Select
            native
            value={timeZone}
            onChange={e => setTimeZone(e.target.value)}
          >
            <option value="" />
            {zones.map((zone) =>
              <option value={zone}>{zone}</option>
            )}
          </Select>
          </FormControl>
        </div>
        <div>
          <Button variant="contained" onClick={onSubmit} color="primary">
            Add
          </Button>
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