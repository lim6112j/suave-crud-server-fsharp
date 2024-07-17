-- CREATE DATABASE mobble
-- DB name mobble
create table mobble_dispatcher.stops_virtual
(
    vstop_idx      integer generated always as identity,
    vstop_name     varchar(100)          not null,
    vstop_loc      varchar(200),
    nearby_link_id integer,
    region_code    integer,
    stops_area_idx integer,
    input_date     varchar(50),
    address        varchar(500),
    geom           varchar(500)          not null,
    is_use         boolean default false not null
);

create table mobble_dispatcher.fare_base
(
    id                           integer generated always as identity,
    fare_once                    integer   default 3000  not null,
    fare_month                   integer   default 50000 not null,
    basic_distance               real      default 1.6   not null,
    basic_distance_fare          integer   default 3000  not null,
    basic_speed                  smallint  default 15    not null,
    basic_combined_time          smallint  default 30    not null,
    basic_combined_time_fare     smallint  default 100   not null,
    basic_combined_distance      smallint  default 131   not null,
    basic_combined_distance_fare smallint  default 100   not null,
    create_time                  timestamp default now() not null,
    update_time                  timestamp
);



create table mobble_dispatcher.tranportation_cost_base
(
    id                        integer generated always as identity,
    diesel_cost               smallint  default 1400   not null,
    electricity_cost          smallint  default 1200   not null,
    man_day_cost              integer   default 187000 not null,
    man_insurance             real      default 14.1   not null,
    vehicle_depreciation_rate smallint  default 10     not null,
    vehicle_insurance         integer   default 200000 not null,
    etc_cost                  integer   default 30000  not null,
    day_run_time              smallint  default 8      not null,
    line_price_distance       integer   default 30000  not null,
    line_price_time           integer   default 150000 not null,
    create_time               timestamp default now()  not null,
    update_time               timestamp
);


create table mobble_dispatcher.fuel_type
(
    id   integer generated always as identity
        primary key,
    type varchar(20)
);



create table mobble_dispatcher.carbon_footprint
(
    id                  integer generated always as identity
        primary key,
    fuel_type_id        integer     default 1                         not null
        constraint fk_fuel_type
            references mobble_dispatcher.fuel_type,
    seats_size          smallint,
    cf_value            smallint,
    fuel_efficiency     smallint,
    vehicle_price       integer,
    cf_unit             varchar(20) default 'g/km'::character varying not null,
    fe_diesel_unit      varchar(20) default 'km/l'::character varying not null,
    fe_electricity_unit varchar(20) default 'km/w'::character varying not null,
    create_time         timestamp   default now()                     not null,
    update_time         timestamp
);