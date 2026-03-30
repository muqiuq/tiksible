# tiksible vlan

Calculates the delta between your desired VLAN configuration and the current state on one or more MikroTik switches, then optionally applies the changes.

## Basic usage

```
tiksible vlan                          # dry-run: print changes, apply nothing
tiksible vlan -e                       # apply changes
tiksible vlan -f switch1               # filter to hosts matching regex "switch1"
tiksible vlan -h hosts.yaml -c creds.yaml
```

| Flag | Default | Description |
|---|---|---|
| `-e` / `--execute` | off | Apply changes (default is dry-run) |
| `-h` / `--hosts` | `hosts.yaml` | Path to hosts file |
| `-c` / `--credentials` | `~/.tiksible-credentials.yaml` | Path to credentials file |
| `-f` / `--filter` | _(all)_ | Regex filter on host names |
| `-v` / `--verbose` | off | Verbose / debug output |

---

## Configuration in hosts.yaml

Each host can have a `vlan:` block under `params:`. Hosts without a `vlan:` block are silently skipped.

### Minimal example — one bridge, two VLANs, eight ports

```yaml
- name: sw-office
  host: 192.168.1.1
  params:
    vlan:
      bridges:
        - name: bridge
          pvid: 1                    # native VLAN of the bridge port itself

      profiles:
        - name: Management
          untagged: 1                # ports get VLAN 1 as their native/untagged VLAN
          tagged: []                 # no tagged VLANs on this profile

        - name: Office
          untagged: 10
          tagged: [20, 30]           # also carry VLANs 20 and 30 tagged

      assignments:
        - ports: "1-4"
          profile: Management
        - ports: "5-8"
          profile: Office
```

Running `tiksible vlan` prints every RouterOS command that would be issued.  
Running `tiksible vlan -e` executes them one by one and reports pass/fail per command.

---

## Profiles in detail

```yaml
profiles:
  - name: Trunk
    comment: "Uplink trunk"         # optional comment applied to ports
    untagged: 1                     # native VLAN (pvid) on the port
    tagged: all                     # "all" → every VLAN in the config except 'untagged'
    edge: "no"                      # RouterOS edge setting (default: auto)
    bpduGuard: "no"                 # RouterOS bpdu-guard (default: no)
    properties:                     # extra RouterOS bridge-port properties
      multicast-router: temporary-query
```

`tagged` accepts:
- `all` — all VLAN IDs defined across all profiles, except the profile's own `untagged` VLAN
- A single integer: `tagged: 20`
- A list: `tagged: [20, 30, 40]`

---

## Bridges in detail

```yaml
bridges:
  - name: bridge
    pvid: 1                         # the bridge interface itself appears untagged for VLAN 1
    properties:                     # extra RouterOS bridge properties
      vlan-filtering: "yes"
      protocol-mode: none
```

If the bridge does not exist yet, tiksible will add it. If it exists, only properties listed under `properties` are compared and updated.

---

## Port ranges

Ports can be specified as integers, strings, or ranges:

```yaml
assignments:
  - ports: [1, 2, 3]        # explicit list
  - ports: [1-8]             # range: ether1 through ether8
  - ports: [ether9]          # explicit interface name
  - ports: 1-4               # single range as a string
```

The default port prefix is `ether`. Override it per-host:

```yaml
vlan:
  defaultPrefix: sfp-sfpplus
  numberOfInterfaces: 4       # used only when tagged: all needs a full port list
```

---

## Inheriting config from another host

Use `inherit` to copy bridges, profiles, and assignments from another host defined in the same hosts file. This is useful when several switches share the same VLAN layout.

```yaml
# Base switch — defines the common layout
- name: sw-base
  host: 192.168.1.1
  params:
    vlan:
      bridges:
        - name: bridge
          pvid: 1
          properties:
            vlan-filtering: "yes"
      profiles:
        - name: Office
          untagged: 10
          tagged: [20, 30]
        - name: Guest
          untagged: 40
          tagged: []
      assignments:
        - ports: [1-2]
          profile: Office

# Second switch — inherits everything and adds its own port assignments
- name: sw-floor2
  host: 192.168.1.2
  params:
    vlan:
      inherit: sw-base           # copies bridges, profiles, assignments from sw-base
      assignments:               # these are appended after the inherited ones
        - ports: [1-8]
          profile: Office
        - ports: [9-12]
          profile: Guest

# Third switch — overrides a profile while keeping the rest
- name: sw-floor3
  host: 192.168.1.3
  params:
    vlan:
      inherit: sw-base
      profiles:
        - name: Office           # same name → overrides the inherited Office profile
          untagged: 10
          tagged: [20, 30, 50]   # VLAN 50 added on this switch only
      assignments:
        - ports: [1-24]
          profile: Office
```

**Merge rules:**
- **Profiles / Bridges** — merged by `name`; a local entry with the same name replaces the inherited one; new names are added.
- **Assignments** — inherited entries come first, local entries are appended. If the same port appears in both, the local assignment takes precedence.
- **`defaultPrefix` / `numberOfInterfaces`** — local value is always used; falls back to inherited `numberOfInterfaces` if not set locally.
- **Circular inheritance** (A → B → A) is detected and raises an error.
